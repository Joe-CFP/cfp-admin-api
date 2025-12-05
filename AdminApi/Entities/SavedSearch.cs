namespace AdminApi.Entities;

public class SavedSearch
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string Name { get; set; }
    public string Query { get; set; }
    public string? OrderBy { get; set; }
    public int? Type { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? ValueMin { get; set; }
    public string? ValueMax { get; set; }
    public List<string>? Regions { get; set; }
    public List<string> Fields { get; set; } = null!;
    public bool Alert { get; set; }
    public DateTime? LastRun { get; set; }
    public DateTime? CreatedAt { get; set; }
    public long CurrentTotal { get; set; }
    public long LastYearTotal { get; set; }
    public long FiveYearTotal { get; set; }
}