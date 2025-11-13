namespace AdminApi.Entities;

#pragma warning disable CS8618
public class MemberPreview
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string CurrentState { get; set; }
    public string SubscriptionName { get; set; }
    public int? OrganisationId { get; set; }
    public string? OrganisationName { get; set; }
}