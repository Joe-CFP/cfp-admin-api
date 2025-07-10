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
    public string DataResidencyString { get; set; }
    public bool Mod_BE { get; set; }
    public bool Mod_Claude { get; set; }
}