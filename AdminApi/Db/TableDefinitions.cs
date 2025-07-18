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