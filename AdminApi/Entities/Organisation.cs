#pragma warning disable CS8618
namespace AdminApi.Entities;

public class Organisation : OrganisationRecord
{
    public List<MemberPreview> Members { get; set; }
    public int MemberCount { get; set; }
}