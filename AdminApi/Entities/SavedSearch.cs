namespace AdminApi.Entities;

public class SavedSearch
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string Name { get; set; } = null!;
    public bool Alert { get; set; }
    public DateTime? LastRun { get; set; }
    public DateTime? CreatedAt { get; set; }
    public SearchSpec Spec { get; set; } = null!;
    public long CurrentTotal { get; set; }
    public long LastYearTotal { get; set; }
    public long FiveYearTotal { get; set; }
}