namespace AdminApi.Entities;

public class Organisation
{
    public int Id { get; set; }
    public string Guid { get; set; }
    public string IndexGuid { get; set; }
    public string Name { get; set; }
    public string ElasticSearchIndex { get; set; }
    public DateTime CreateDate { get; set; }
    public bool IsReindexing { get; set; }
    public DateTime AnchorDate { get; set; }
    public int Quota { get; set; }
    public string OptionsString { get; set; }
    public int Version { get; set; }
    public string DataResidency { get; set; }
    public bool HasBidEvaluationModel { get; set; }
    public bool HasClaudeModule { get; set; }
    public List<Member> Members { get; set; }
}

public class OrganisationSearchResult
{
    public int Id { get; set; }
    public string Name { get; set; }
}