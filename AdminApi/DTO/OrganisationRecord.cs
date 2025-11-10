// ReSharper disable PropertyCanBeMadeInitOnly.Global
#pragma warning disable CS8618

namespace AdminApi.Entities;

public class OrganisationRecord
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

    public Organisation ToOrganisation(List<MemberPreview>? members, MemberActivity? activity)
    {
        Organisation organisation = new();
        PropertyMapper.CopyMatchingProperties(this, organisation);
        organisation.Members = members ?? new List<MemberPreview>();
        organisation.Activity = activity;
        return organisation;
    }
}