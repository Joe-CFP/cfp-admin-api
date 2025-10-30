using AdminApi.Entities;

namespace AdminApi.DTO;

public class MemberOptionsRecord
{
    public int MemberId { get; set; }
    public string? SubscriptionCode { get; set; }
    public bool WeeklyEmail { get; set; }
    public bool WeeklyCsv { get; set; }
    public bool DailyEmail { get; set; }
    public bool FlashEmail { get; set; }
    public string? Cc1 { get; set; }
    public string? Cc2 { get; set; }
    public string? Cc3 { get; set; }
    public string? Cc4 { get; set; }
    public string? LanguageList { get; set; }

    public MemberOptions ToMemberOptions()
    {
        MemberOptions dto = new();
        PropertyMapper.CopyMatchingProperties(this, dto);
        return dto;
    }
}