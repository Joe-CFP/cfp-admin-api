using AdminApi.Entities;
using AdminApi.Lib;
using AdminApi.Services;
using OpenSearch.Client;

namespace AdminApi.Repositories;

public interface IOpenSearchRepository
{
    Task<long> CountAsync(SearchSpec spec, CancellationToken cancellationToken);
}

public class OpenSearchRepository : IOpenSearchRepository
{
    private readonly OpenSearchClient _client;
    private readonly IQueryParsingService _queryParser;
    private readonly IOpenSearchQueryCompiler _queryCompiler;

    public OpenSearchRepository(ISecretStore secrets, IQueryParsingService queryParser, IOpenSearchQueryCompiler queryCompiler)
    {
        _queryParser = queryParser;
        _queryCompiler = queryCompiler;

        Secret os = secrets[SecretName.ProdCoreOpensearch];
        ConnectionSettings settings = new ConnectionSettings(new Uri(os.Endpoint))
            .BasicAuthentication(os.Username, os.Password)
            .ThrowExceptions()
            .PrettyJson()
            .DisableDirectStreaming()
            .MaximumRetries(3)
            .RequestTimeout(TimeSpan.FromSeconds(10))
            .MaxRetryTimeout(TimeSpan.FromSeconds(30))
            .DefaultFieldNameInferrer(p => p);
        _client = new OpenSearchClient(settings);
    }

    public async Task<long> CountAsync(SearchSpec spec, CancellationToken cancellationToken)
    {
        string indexName = spec.Index == IndexKind.Global ? "docindex*" : "docindex";

        CountResponse response = await _client.CountAsync<object>(c => c
            .Index(indexName)
            .Query(q => BuildQuery(q, spec)),
            cancellationToken);

        return response.Count;
    }

    private QueryContainer BuildQuery(QueryContainerDescriptor<object> q, SearchSpec spec)
    {
        QueryContainer query = q.MatchAll();

        if (!string.IsNullOrWhiteSpace(spec.Query))
        {
            string[] fields = ResolveTermFields(spec);
            ParsedQuery parsed = _queryParser.Parse(spec.Query);

            if (parsed.ExpressionTree is not null)
                query &= _queryCompiler.Compile(parsed.ExpressionTree, fields);
        }

        if (spec.PublishedFromUtc is not null || spec.PublishedToUtc is not null)
            query &= q.DateRange(d => ApplyDateRange(d.Field("publisheddate"), spec.PublishedFromUtc, spec.PublishedToUtc));

        if (spec.ClosingOnOrAfterUtc is not null)
            query &= q.DateRange(d => d.Field("closingdate").GreaterThanOrEquals(spec.ClosingOnOrAfterUtc.Value));

        if (spec.Types is { Count: > 0 })
        {
            int[] typeInts = spec.Types.Select(t => (int)t).ToArray();
            query &= q.Terms(t => t.Field("typeint").Terms(typeInts));
        }

        if (spec.Regions is { Count: > 0 })
            query &= q.Terms(t => t.Field("nuts").Terms(spec.Regions.ToArray()));

        if (spec.ValueMin is not null || spec.ValueMax is not null)
            query &= q.Range(r => ApplyValueRange(r.Field("valuemax"), spec.ValueMin, spec.ValueMax));

        return query;
    }

    private static string[] ResolveTermFields(SearchSpec spec)
    {
        if (spec.QueryFields is { Count: > 0 })
            return spec.QueryFields.Select(ToOsFieldName).ToArray();

        QueryFieldSet set = spec.QueryFieldSet ?? QueryFieldSet.Default;
        return set switch {
            QueryFieldSet.Default => new[] { "summary", "reftitleshort", "publisher", "awardedtofirstlines", "cpvdesc", "cpvcodes", "location" },
            _ => new[] { "summary", "reftitleshort", "publisher", "awardedtofirstlines", "cpvdesc", "cpvcodes", "location" }
        };
    }

    private static string ToOsFieldName(SearchField f)
    {
        return f switch {
            SearchField.Summary => "summary",
            SearchField.RefTitleShort => "reftitleshort",
            SearchField.Publisher => "publisher",
            SearchField.AwardedToFirstLines => "awardedtofirstlines",
            SearchField.CpvDesc => "cpvdesc",
            SearchField.CpvCodes => "cpvcodes",
            SearchField.Location => "location",
            _ => "summary"
        };
    }

    private static DateRangeQueryDescriptor<object> ApplyDateRange(
        DateRangeQueryDescriptor<object> d,
        DateTime? fromUtc,
        DateTime? toUtc)
    {
        if (fromUtc is not null)
            d = d.GreaterThanOrEquals(fromUtc.Value);

        if (toUtc is not null)
            d = d.LessThanOrEquals(toUtc.Value);

        return d;
    }

    private static NumericRangeQueryDescriptor<object> ApplyValueRange(
        NumericRangeQueryDescriptor<object> r,
        decimal? min,
        decimal? max)
    {
        if (min is not null)
            r = r.GreaterThanOrEquals(Convert.ToDouble(min.Value));

        if (max is not null)
            r = r.LessThanOrEquals(Convert.ToDouble(max.Value));

        return r;
    }
}
