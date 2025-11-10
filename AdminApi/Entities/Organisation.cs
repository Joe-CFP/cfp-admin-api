#pragma warning disable CS8618
namespace AdminApi.Entities;

public class Organisation : OrganisationRecord
{
    public List<MemberPreview> Members { get; set; }
    public MemberActivity? Activity { get; set; }
}

public class OrganisationSearchResult
{
    public int Id { get; set; }
    public string Name { get; set; }
}