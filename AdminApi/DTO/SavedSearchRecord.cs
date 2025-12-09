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

        return new() {
            Id = Id,
            MemberId = MemberId,
            Name = SearchName ?? p?.Name ?? "",
            Query = p?.Query ?? "",
            OrderBy = p?.OrderBy,
            Type = p?.TypeInt,
            DateFrom = Normalize(p?.MinDate),
            DateTo = Normalize(p?.MaxDate),
            ValueMin = p?.MinValueStr,
            ValueMax = p?.MaxValueStr,
            Regions = p?.Nuts?.ToList(),
            Fields = p?.ShortFields?.ToList(),
            Alert = Alert,
            LastRun = Normalize(LastRun),
            CreatedAt = Normalize(InsertTime)
        };
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
