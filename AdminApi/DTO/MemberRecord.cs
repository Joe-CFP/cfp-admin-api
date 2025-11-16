// ReSharper disable PropertyCanBeMadeInitOnly.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace AdminApi.Entities;

public class MemberRecord
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public bool IsVerified { get; set; }
    public DateTime ModificationDate { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address { get; set; }
    public string Postcode { get; set; }
    public string RegisterCode { get; set; }
    public string PasswordCode { get; set; }
    public string StripeCustomerId { get; set; }
    public string StripeSubscriptionId { get; set; }
    public string StripePlanIdLegacy { get; set; }
    public DateTime LastLoginDate { get; set; }
    public DateTime LoginExpiryDate { get; set; }
    public string ReferralCode { get; set; }
    public bool InRegister { get; set; }
    public string? OrganisationGuid { get; set; }
    public int? OrganisationId { get; set; }
    public string? OrganisationName { get; set; }
    public int OrganisationMemberCount { get; set; }
    public string? CurrentState { get; set; }

    public static readonly Dictionary<string, string> SubscriptionNameFromCode = new()
    {
        { "EURO", "Euro" },
        { "STAN", "Legacy" },
        { "PREM", "Pro" },
        { "BASI", "Basic" }
    };

    private static string SubscriptionCasesSql =>
        string.Join("\n", SubscriptionNameFromCode.Select(kv => $"WHEN '{kv.Key}' THEN '{kv.Value}'"));

    public static string SubscriptionCaseSql =>
        $"CASE mo.subtype {SubscriptionCasesSql} ELSE 'Unknown' END AS SubscriptionName";

    public Member ToMember(MemberOptions? memberOptions, DateTime? estimatedRegistrationDate)
    {
        Member member = new();
        PropertyMapper.CopyMatchingProperties(this, member);
        if (memberOptions != null)
            PropertyMapper.CopyMatchingProperties(memberOptions, member);
        member.SubscriptionName = SubscriptionNameFromCode.GetValueOrDefault(member.SubscriptionCode, "Unknown");
        member.RegistrationDate = estimatedRegistrationDate;
        return member;
    }
}
