using System.Globalization;
using System.Text.Json;
using AdminApi.Entities;
using AdminApi.Legacy;

namespace AdminApi.DTO;

public class SavedSearchRecord
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string? SearchJson { get; set; }
    public string? SearchName { get; set; }
    public bool Alert { get; set; }
    public DateTime? LastRun { get; set; }
    public DateTime? InsertTime { get; set; }

    public SavedSearch ToSavedSearch()
    {
        SavedSearchPayload? p = null;
        if (!string.IsNullOrWhiteSpace(SearchJson))
        {
            var opts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new LegacyDateTimeConverter() }
            };
            try { p = JsonSerializer.Deserialize<SavedSearchPayload>(SearchJson, opts); }
            catch { }
        }

        DateTime? Normalize(DateTime? d)
        {
            if (d == null) return null;
            if (d == DateTime.MinValue) return null;
            if (d.Value.Year < 1900) return null;
            return d;
        }

        SearchSpec spec = BuildSpec(p);

        return new() {
            Id = Id,
            MemberId = MemberId,
            Name = SearchName ?? p?.Name ?? "",
            Alert = Alert,
            LastRun = Normalize(LastRun),
            CreatedAt = Normalize(InsertTime),
            Spec = spec
        };
    }

    private static SearchSpec BuildSpec(SavedSearchPayload? p)
    {
        IndexKind index = DetermineIndex(p?.Nuts);

        QueryFieldSet? fieldSet = string.IsNullOrWhiteSpace(p?.Query) ? null : QueryFieldSet.Default;

        IReadOnlyList<NoticeType>? types = null;
        if (p?.TypeInt is not null)
        {
            if (p.TypeInt.Value == (int)NoticeType.Tender)
                types = new[] { NoticeType.Tender, NoticeType.Pin };
            else if (Enum.IsDefined(typeof(NoticeType), p.TypeInt.Value))
                types = new[] { (NoticeType)p.TypeInt.Value };
        }

        IReadOnlyList<string>? regions = null;
        if (p?.Nuts is not null && p.Nuts.Length > 0)
        {
            regions = p.Nuts
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Append("u")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        decimal? min = TryParseDecimal(p?.MinValueStr, out decimal parsedMin) ? parsedMin : null;
        decimal? max = TryParseDecimalUpper(p?.MaxValueStr, out decimal parsedMax) ? parsedMax : null;

        return new(
            Index: index,
            Query: p?.Query,
            QueryFieldSet: fieldSet,
            QueryFields: null,
            Types: types,
            Regions: regions,
            PublishedFromUtc: null,
            PublishedToUtc: null,
            ClosingOnOrAfterUtc: null,
            ValueMin: min,
            ValueMax: max,
            OrderBy: null,
            PageSize: null
        );
    }

    private static IndexKind DetermineIndex(string[]? nuts)
    {
        if (nuts is null || nuts.Length == 0) return IndexKind.Uk;

        bool global = nuts.Any(r =>
            string.Equals(r, "eu", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, "os", StringComparison.OrdinalIgnoreCase));

        return global ? IndexKind.Global : IndexKind.Uk;
    }

    private static bool TryParseDecimal(string? value, out decimal result)
    {
        result = 0m;
        if (string.IsNullOrWhiteSpace(value)) return false;
        string trimmed = value.Trim();
        if (string.Equals(trimmed, "max", StringComparison.OrdinalIgnoreCase)) return false;
        return decimal.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private static bool TryParseDecimalUpper(string? value, out decimal result)
    {
        result = 0m;
        if (string.IsNullOrWhiteSpace(value)) return false;
        string trimmed = value.Trim();
        if (string.Equals(trimmed, "max", StringComparison.OrdinalIgnoreCase)) return false;
        return decimal.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    class SavedSearchPayload
    {
        public string? Name { get; set; }
        public string? OrderBy { get; set; }
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
        public int? TypeInt { get; set; }
        public string? MinValueStr { get; set; }
        public string? MaxValueStr { get; set; }
        public string? Query { get; set; }
        public string[]? Nuts { get; set; }
        public string[]? ShortFields { get; set; }
    }
}
