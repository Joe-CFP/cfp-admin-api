#pragma warning disable CS8618
namespace AdminApi.Entities;

public class Member: MemberRecord
{
    public string? CurrentState { get; set; }
    public string SubscriptionCode { get; set; }
    public string SubscriptionName { get; set; }
    public bool DailyEmail { get; set; }
    public bool WeeklyEmail { get; set; }
    public bool FlashEmail { get; set; }
    public bool WeeklyCsv { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public int OrganisationMemberCount { get; set; }
}
