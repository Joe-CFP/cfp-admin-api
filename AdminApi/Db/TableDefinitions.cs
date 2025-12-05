using System.Linq.Expressions;
using AdminApi.DTO;
using AdminApi.Entities;

namespace AdminApi.Db;

public static class TableDefinitions
{
    public static readonly DbTable OrganisationTable = new()
    {
        Name = "Organisation",
        DbName = "kborgindex",
        Type = typeof(OrganisationRecord),
        Fields = new() {
            FieldFor<OrganisationRecord, int>(o => o.Id, "orgid", "Primary key", "int"),
            FieldFor<OrganisationRecord, string>(o => o.Name, "orgname", "Name of the organisation", "varchar", 500),
            FieldFor<OrganisationRecord, string>(o => o.Guid, "orgguid", "Unique identifier for organisation", "varchar", 45),
            FieldFor<OrganisationRecord, string>(o => o.IndexGuid, "indexguid", "Unique identifier for organisation's elastic search index", "varchar", 45),
            FieldFor<OrganisationRecord, string>(o => o.ElasticSearchIndex, "orgindex", "Searchable index name", "varchar", 200),
            FieldFor<OrganisationRecord, DateTime>(o => o.CreateDate, "createdate", "Date the organisation was created", "datetime"),
            FieldFor<OrganisationRecord, bool>(o => o.IsReindexing, "inreindex", "True if organisation is currently being reindexed (may not auto-clear)", "tinyint"),
            FieldFor<OrganisationRecord, DateTime>(o => o.AnchorDate, "anchordate", "Anchor date for model resets or analysis", "datetime"),
            FieldFor<OrganisationRecord, int>(o => o.Quota, "quota", "Monthly token quota (Table 'tagptcosts' holds usage)", "int"),
            FieldFor<OrganisationRecord, string>(o => o.OptionsString, "optionsstring", "Miscellaneous options in serialized string format", "mediumtext", 16777215),
            FieldFor<OrganisationRecord, int>(o => o.Version, "version", "Data version or schema version", "int"),
            FieldFor<OrganisationRecord, string>(o => o.DataResidency, "dataresidencystring", "Data residency (global, eu, or uk)", "varchar", 60),
            FieldFor<OrganisationRecord, bool>(o => o.HasBidEvaluationModule, "mod_be", "Whether Bid Evaluation module is enabled", "tinyint"),
            FieldFor<OrganisationRecord, bool>(o => o.HasClaudeModule, "mod_claude", "Whether Claude module is enabled", "tinyint"),
            FieldFor<OrganisationRecord, bool>(o => o.HasSharePointModule, "mod_sp", "Whether SharePoint module is enabled", "tinyint"),
            FieldFor<OrganisationRecord, bool>(o => o.HasLabsModule, "mod_labs", "Whether Labs module is enabled", "tinyint"),
            FieldFor<OrganisationRecord, bool>(o => o.HasBetaModule, "mod_beta", "Whether Beta features are enabled", "tinyint"),
        }
    };

    public static readonly DbTable MemberTable = new()
    {
        Name = "Member",
        DbName = "members",
        Type = typeof(MemberRecord),
        Fields = new() {
            FieldFor<MemberRecord, int>(m => m.Id, "id", "Primary key", "int"),
            FieldFor<MemberRecord, string>(m => m.Username, "username", "Username (Same as Email)", "varchar", 65),
            FieldFor<MemberRecord, string>(m => m.Email, "email", "Email address", "varchar", 65),
            FieldFor<MemberRecord, bool>(m => m.IsVerified, "verified", "Whether account is verified", "tinyint"),
            FieldFor<MemberRecord, DateTime>(m => m.ModificationDate, "mod_timestamp", "Last modified timestamp", "datetime"),
            FieldFor<MemberRecord, string>(m => m.FirstName, "firstname", "First name", "varchar", 45),
            FieldFor<MemberRecord, string>(m => m.LastName, "lastname", "Last name", "varchar", 45),
            FieldFor<MemberRecord, string>(m => m.Address, "address", "Postal address", "varchar", 200),
            FieldFor<MemberRecord, string>(m => m.Postcode, "postcode", "Postal code", "varchar", 45),
            FieldFor<MemberRecord, string>(m => m.RegisterCode, "registercode", "Registration verification code", "varchar", 100),
            FieldFor<MemberRecord, string>(m => m.StripeCustomerId, "custid", "Stripe customer ID", "varchar", 100),
            FieldFor<MemberRecord, string>(m => m.StripeSubscriptionId, "subid", "Stripe Subscription ID", "varchar", 100),
            FieldFor<MemberRecord, string>(m => m.StripePlanIdLegacy, "planid", "Stripe Plan ID - No longer in use", "varchar", 100),
            FieldFor<MemberRecord, DateTime>(m => m.LastLoginDate, "last_login", "Last login timestamp", "datetime"),
            FieldFor<MemberRecord, DateTime>(m => m.LoginExpiryDate, "login_expiry", "Login expiry timestamp", "datetime"),
            FieldFor<MemberRecord, string>(m => m.ReferralCode, "refercode", "Referral code", "varchar", 100),
            FieldFor<MemberRecord, bool>(m => m.InRegister, "inregister", "???", "tinyint"),
            FieldFor<MemberRecord, string>(m => m.OrganisationGuid, "orgguid", "Organisation", "varchar", 45),
        }
    };

    public static readonly DbTable UserJourneyTable = new()
    {
        Name = "UserJourney",
        DbName = "userjourney",
        Type = typeof(UserJourneyRecord),
        Fields = new() {
            FieldFor<UserJourneyRecord, int>(u => u.UserJourneyId, "ujid", "Primary key", "int"),
            FieldFor<UserJourneyRecord, string>(u => u.Username, "username", "Username", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.CurrentState, "curstate", "Current state", "varchar", 100),
            FieldFor<UserJourneyRecord, DateTime>(u => u.CurrentStateDateTime, "curstatedatetime", "Current state datetime", "datetime"),
            FieldFor<UserJourneyRecord, string>(u => u.AutoNextState, "autonextstate", "Next state", "varchar", 100),
            FieldFor<UserJourneyRecord, DateTime?>(u => u.AutoNextStateDateTime, "autonextstatedatetime", "Next state datetime", "datetime"),
            FieldFor<UserJourneyRecord, string>(u => u.Summary, "summary", "Summary", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.HistoryJson, "historyjson", "History JSON", "text", 65535),
            FieldFor<UserJourneyRecord, string>(u => u.Action1, "action1", "Action 1", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action2, "action2", "Action 2", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action3, "action3", "Action 3", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action4, "action4", "Action 4", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action5, "action5", "Action 5", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action6, "action6", "Action 6", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action7, "action7", "Action 7", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action8, "action8", "Action 8", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action9, "action9", "Action 9", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action10, "action10", "Action 10", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action11, "action11", "Action 11", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action12, "action12", "Action 12", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action13, "action13", "Action 13", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action14, "action14", "Action 14", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action15, "action15", "Action 15", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action16, "action16", "Action 16", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action17, "action17", "Action 17", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action18, "action18", "Action 18", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action19, "action19", "Action 19", "varchar", 500),
            FieldFor<UserJourneyRecord, string>(u => u.Action20, "action20", "Action 20", "varchar", 500),
        }
    }; 

    public static readonly DbTable MemberOptionsTable = new()
    {
        Name = "MemberOptions",
        DbName = "memberoptions",
        Type = typeof(MemberOptionsRecord),
        Fields = new() {
            FieldFor<MemberOptionsRecord, int>(m => m.MemberId, "id", "Primary key", "int"),
            FieldFor<MemberOptionsRecord, string?>(m => m.SubscriptionCode, "subtype", "Subscription type", "varchar", 45),
            FieldFor<MemberOptionsRecord, bool>(m => m.WeeklyEmail, "weeklyemail", "Weekly email enabled", "tinyint"),
            FieldFor<MemberOptionsRecord, bool>(m => m.WeeklyCsv, "weeklycsv", "Weekly CSV enabled", "tinyint"),
            FieldFor<MemberOptionsRecord, bool>(m => m.DailyEmail, "dailyemail", "Daily email enabled", "tinyint"),
            FieldFor<MemberOptionsRecord, bool>(m => m.FlashEmail, "flashemail", "Flash email enabled", "tinyint"),
            FieldFor<MemberOptionsRecord, string?>(m => m.Cc1, "cc1", "Additional email recipient 1", "varchar", 200),
            FieldFor<MemberOptionsRecord, string?>(m => m.Cc2, "cc2", "Additional email recipient 2", "varchar", 200),
            FieldFor<MemberOptionsRecord, string?>(m => m.Cc3, "cc3", "Additional email recipient 3", "varchar", 200),
            FieldFor<MemberOptionsRecord, string?>(m => m.Cc4, "cc4", "Additional email recipient 4", "varchar", 200),
            FieldFor<MemberOptionsRecord, string?>(m => m.LanguageList, "langlist", "Preferred language(s)", "varchar", 30),
        }
    };

    public static readonly DbTable SavedSearchTable = new() {
        Name = "SavedSearch",
        DbName = "search",
        Type = typeof(AdminApi.DTO.SavedSearchRecord),
        Fields = new() {
            FieldFor<AdminApi.DTO.SavedSearchRecord, int>(s => s.Id, "searchid", "Primary key", "int"),
            FieldFor<AdminApi.DTO.SavedSearchRecord, int>(s => s.MemberId, "userid", "Member id", "int"),
            FieldFor<AdminApi.DTO.SavedSearchRecord, string>(s => s.SearchJson, "json", "Search JSON", "text", 65535),
            FieldFor<AdminApi.DTO.SavedSearchRecord, string>(s => s.SearchName, "searchname", "Search name", "varchar",
                45),
            FieldFor<AdminApi.DTO.SavedSearchRecord, bool>(s => s.Alert, "alert", "Alert flag", "tinyint"),
            FieldFor<AdminApi.DTO.SavedSearchRecord, DateTime?>(s => s.LastRun, "lastrun", "Last run", "datetime"),
            FieldFor<AdminApi.DTO.SavedSearchRecord, DateTime?>(s => s.InsertTime, "inserttime", "Inserted at",
                "datetime"),
        }
    };
   
    private static DbField FieldFor<T, TProp>(Expression<Func<T, TProp>> expr,
        string fieldName, string description, string fieldType, int? maxLength = null)
    {
        if (expr.Body is not MemberExpression member)
            throw new ArgumentException("Expression must be a member access", nameof(expr));

        return new() {
            Name = member.Member.Name,
            DbName = fieldName,
            Description = description,
            Type = typeof(TProp),
            DbType = fieldType,
            MaxLength = maxLength
        };
    }
}