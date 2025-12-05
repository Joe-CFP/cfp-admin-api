using AdminApi.Entities;
using AdminApi.Lib;
using OpenSearch.Client;

namespace AdminApi.Repositories
{
    public interface IOpenSearchRepository
    {
        Task<long> CountAsync(SavedSearch search, DateTime from, DateTime to, bool mustBeOpen, CancellationToken cancellationToken);
        Task<long> CountCurrentAsync(SavedSearch search, CancellationToken cancellationToken);
        Task<long> CountLastYearAsync(SavedSearch search, CancellationToken cancellationToken);
        Task<long> CountLastFiveYearsAsync(SavedSearch search, CancellationToken cancellationToken);
        Task<(long Current, long LastYear, long FiveYears)> GetCountsAsync(SavedSearch search, CancellationToken cancellationToken);
    }

    public class OpenSearchRepository : IOpenSearchRepository
    {
        private readonly OpenSearchClient _client;

        public OpenSearchRepository(ISecretStore secrets)
        {
            Secret os = secrets[SecretName.ProdCoreOpensearch];
            ConnectionSettings settings = new ConnectionSettings(new Uri(os.Endpoint))
                .BasicAuthentication(os.Username, os.Password)
                .ThrowExceptions()
                .PrettyJson()
                .DisableDirectStreaming()
                .DefaultFieldNameInferrer(p => p);
            _client = new OpenSearchClient(settings);
        }

        public Task<long> CountCurrentAsync(SavedSearch search, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.UtcNow;
            DateTime from = now.AddYears(-1);
            DateTime to = now;
            bool mustBeOpen = search.Type == 1;
            return CountAsync(search, from, to, mustBeOpen, cancellationToken);
        }

        public Task<long> CountLastYearAsync(SavedSearch search, CancellationToken cancellationToken)
        {
            DateTime to = DateTime.UtcNow;
            DateTime from = to.AddYears(-1);
            bool mustBeOpen = false;
            return CountAsync(search, from, to, mustBeOpen, cancellationToken);
        }

        public Task<long> CountLastFiveYearsAsync(SavedSearch search, CancellationToken cancellationToken)
        {
            DateTime to = DateTime.UtcNow;
            DateTime from = to.AddYears(-5);
            bool mustBeOpen = false;
            return CountAsync(search, from, to, mustBeOpen, cancellationToken);
        }

        public async Task<long> CountAsync(SavedSearch search, DateTime from, DateTime to, bool mustBeOpen, CancellationToken cancellationToken)
        {
            string indexName = DetermineIndexName(search);
            CountResponse response = await _client.CountAsync<object>(c => c
                .Index(indexName)
                .Query(q => BuildQuery(q, search, from, to, mustBeOpen)),
                cancellationToken);
            return response.Count;
        }

        private QueryContainer BuildQuery(QueryContainerDescriptor<object> q, SavedSearch search, DateTime from, DateTime to, bool mustBeOpen)
        {
            QueryContainer queryContainer = q.MatchAll();

            if (!string.IsNullOrWhiteSpace(search.Query))
            {
                queryContainer &= q.SimpleQueryString(s => s
                    .Query(search.Query)
                    .DefaultOperator(Operator.Or)
                    .Fields(f => ApplyFields(f, search)));
            }

            queryContainer &= q.DateRange(d => d.Field("publisheddate").GreaterThanOrEquals(from).LessThanOrEquals(to));

            int type = search.Type ?? 0;
            if (type == 1)
            {
                queryContainer &= q.Terms(t => t.Field("typeint").Terms(new[] { 1, 3 }));
                if (mustBeOpen)
                    queryContainer &= q.DateRange(d => d.Field("closingdate").GreaterThanOrEquals(DateTime.UtcNow.Date));
            }
            else if (type >= 2)
            {
                queryContainer &= q.Term(t => t.Field("typeint").Value(type));
            }

            if (search.Regions is not null && search.Regions.Count > 0)
            {
                string[] regionsWithU = search.Regions.Concat(["u"]).Distinct().ToArray();
                queryContainer &= q.Terms(t => t.Field("nuts").Terms(regionsWithU));
            }

            if (TryParseDecimal(search.ValueMin, out decimal valueMin))
            {
                queryContainer &= q.Range(r => r.Field("valuemax").GreaterThanOrEquals(Convert.ToDouble(valueMin)));
            }

            if (TryParseDecimalUpper(search.ValueMax, out decimal valueMax))
            {
                queryContainer &= q.Range(r => r.Field("valuemax").LessThanOrEquals(Convert.ToDouble(valueMax)));
            }

            return queryContainer;
        }

        private FieldsDescriptor<object> ApplyFields(FieldsDescriptor<object> fields, SavedSearch search)
        {
            string[] fieldNames = search is { Fields.Count: > 0 }
                ? GetSearchFieldList(search)
                : GetDefaultFieldList();

            return fieldNames.Aggregate(fields, (current, t) => current.Field(t));
        }

        private static string[] GetSearchFieldList(SavedSearch search)
        {
            string[] mapped = search.Fields.Select(ShortFieldToLong).ToArray();
            return mapped.Length > 0 ? mapped : GetDefaultFieldList();
        }

        private static string[] GetDefaultFieldList()
        {
            return ["summary", "reftitleshort", "publisher", "awardedtofirstlines", "cpvdesc", "cpvcodes", "location"];
        }

        private static string ShortFieldToLong(string code)
        {
            string key = code.Trim().ToLowerInvariant();
            return key switch {
                "pu" => "publisher",
                "lo" => "location",
                "su" => "summary",
                "cp" => "cpvdesc",
                "ti" => "reftitleshort",
                "aw" => "awardedtofirstlines",
                "rl" => "procedurelanguagescodes",
                _ => "summary"
            };
        }

        private static bool TryParseDecimal(string? value, out decimal result)
        {
            result = 0m;
            if (string.IsNullOrWhiteSpace(value)) return false;
            string trimmed = value.Trim();
            if (string.Equals(trimmed, "max", StringComparison.OrdinalIgnoreCase)) return false;
            return decimal.TryParse(trimmed, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result);
        }

        private static bool TryParseDecimalUpper(string? value, out decimal result)
        {
            result = 0m;
            if (string.IsNullOrWhiteSpace(value)) return false;
            string trimmed = value.Trim();
            if (string.Equals(trimmed, "max", StringComparison.OrdinalIgnoreCase)) return false;
            return decimal.TryParse(trimmed, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result);
        }

        private static string DetermineIndexName(SavedSearch search)
        {
            if (search.Regions is not null && search.Regions.Any(r => string.Equals(r, "eu", StringComparison.OrdinalIgnoreCase) || string.Equals(r, "os", StringComparison.OrdinalIgnoreCase)))
                return "docindex*";
            return "docindex";
        }
        
        public async Task<(long Current, long LastYear, long FiveYears)> GetCountsAsync(
            SavedSearch search,
            CancellationToken cancellationToken)
        {
            long current = await CountCurrentAsync(search, cancellationToken);
            long lastYear = await CountLastYearAsync(search, cancellationToken);
            long fiveYears = await CountLastFiveYearsAsync(search, cancellationToken);
            return (current, lastYear, fiveYears);
        }
    }
}
