#pragma warning disable CS8618
namespace AdminApi.Entities;

public class MemberOptions
{
    public int MemberId { get; set; }
    public string SubscriptionCode { get; set; }
    public bool WeeklyEmail { get; set; }
    public bool WeeklyCsv { get; set; }
    public bool DailyEmail { get; set; }
    public bool FlashEmail { get; set; }
}