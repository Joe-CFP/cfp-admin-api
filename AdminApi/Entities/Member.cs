namespace AdminApi.Entities;

public class Member
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
    public string OrganisationGuid { get; set; }
}