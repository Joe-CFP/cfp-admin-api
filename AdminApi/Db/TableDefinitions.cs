using System.Linq.Expressions;
using AdminApi.Entities;

namespace AdminApi.Db;

public static class TableDefinitions
{
    public static readonly DbTable OrganisationTable = new()
    {
        Name = "Organisation",
        DbName = "kborgindex",
        Type = typeof(Organisation),
        Fields = new List<DbField>
        {
            FieldFor<Organisation, int>(o => o.Id, "orgid", "Primary key", "int"),
            FieldFor<Organisation, string>(o => o.Name, "orgname", "Name of the organisation", "varchar", 500),
            FieldFor<Organisation, string>(o => o.Guid, "orgguid", "Unique identifier for organisation", "varchar", 45),
            FieldFor<Organisation, string>(o => o.IndexGuid, "indexguid", "Unique identifier for organisation's elastic search index", "varchar", 45),
            FieldFor<Organisation, string>(o => o.ElasticSearchIndex, "orgindex", "Searchable index name", "varchar", 200),
            FieldFor<Organisation, DateTime>(o => o.CreateDate, "createdate", "Date the organisation was created", "datetime"),
            FieldFor<Organisation, bool>(o => o.IsReindexing, "inreindex", "True if organisation is currently being reindexed (may not auto-clear)", "tinyint"),
            FieldFor<Organisation, DateTime>(o => o.AnchorDate, "anchordate", "Anchor date for model resets or analysis", "datetime"),
            FieldFor<Organisation, int>(o => o.Quota, "quota", "Monthly token quota (Table 'tagptcosts' holds usage)", "int"),
            FieldFor<Organisation, string>(o => o.OptionsString, "optionsstring", "Miscellaneous options in serialized string format", "mediumtext", 16777215),
            FieldFor<Organisation, int>(o => o.Version, "version", "Data version or schema version", "int"),
            FieldFor<Organisation, string>(o => o.DataResidency, "dataresidencystring", "Data residency (global, eu, or uk)", "varchar", 60),
            FieldFor<Organisation, bool>(o => o.HasBidEvaluationModel, "mod_be", "Whether Bid Evaluation module is enabled", "tinyint"),
            FieldFor<Organisation, bool>(o => o.HasClaudeModule, "mod_claude", "Whether Claude module is enabled", "tinyint")
        }
    };

    public static readonly DbTable MemberTable = new()
    {
        Name = "Member",
        DbName = "members",
        Type = typeof(Member),
        Fields = new List<DbField>
        {
            FieldFor<Member, int>(m => m.Id, "id", "Primary key", "int"),
            FieldFor<Member, string>(m => m.Username, "username", "Username (Same as Email)", "varchar", 65),
            FieldFor<Member, string>(m => m.Password, "password", "Hashed password", "varchar", 65),
            FieldFor<Member, string>(m => m.Email, "email", "Email address", "varchar", 65),
            FieldFor<Member, bool>(m => m.IsVerified, "verified", "Whether account is verified", "tinyint"),
            FieldFor<Member, DateTime>(m => m.ModificationDate, "mod_timestamp", "Last modified timestamp", "datetime"),
            FieldFor<Member, string>(m => m.FirstName, "firstname", "First name", "varchar", 45),
            FieldFor<Member, string>(m => m.LastName, "lastname", "Last name", "varchar", 45),
            FieldFor<Member, string>(m => m.Address, "address", "Postal address", "varchar", 200),
            FieldFor<Member, string>(m => m.Postcode, "postcode", "Postal code", "varchar", 45),
            FieldFor<Member, string>(m => m.RegisterCode, "registercode", "Registration verification code", "varchar", 100),
            FieldFor<Member, string>(m => m.PasswordCode, "passwordcode", "Password reset code", "varchar", 100),
            FieldFor<Member, string>(m => m.StripeCustomerId, "custid", "Stripe customer ID", "varchar", 100),
            FieldFor<Member, string>(m => m.StripeSubscriptionId, "subid", "Stripe Subscription ID", "varchar", 100),
            FieldFor<Member, string>(m => m.StripePlanIdLegacy, "planid", "Stripe Plan ID - No longer in use", "varchar", 100),
            FieldFor<Member, DateTime>(m => m.LastLoginDate, "last_login", "Last login timestamp", "datetime"),
            FieldFor<Member, DateTime>(m => m.LoginExpiryDate, "login_expiry", "Login expiry timestamp", "datetime"),
            FieldFor<Member, string>(m => m.ReferralCode, "refercode", "Referral code", "varchar", 100),
            FieldFor<Member, bool>(m => m.InRegister, "inregister", "???", "tinyint"),
            FieldFor<Member, string>(m => m.OrganisationGuid, "orgguid", "Organisation", "varchar", 45)
        }
    };

    private static DbField FieldFor<T, TProp>(Expression<Func<T, TProp>> expr, 
        string fieldName, string description, string fieldType, int? maxLength = null)
    {
        if (expr.Body is not MemberExpression member)
            throw new ArgumentException("Expression must be a member access", nameof(expr));

        return new DbField
        {
            Name = member.Member.Name,
            DbName = fieldName,
            Description = description,
            Type = typeof(TProp),
            DbType = fieldType,
            MaxLength = maxLength
        };
    }
}