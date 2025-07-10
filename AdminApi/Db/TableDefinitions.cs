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
            FieldFor<Organisation, int>(o => o.Id, "orgid", "int", "Primary key"),
            FieldFor<Organisation, string>(o => o.Guid, "orgguid", "varchar", "Unique identifier for organisation", 45),
            FieldFor<Organisation, string>(o => o.IndexGuid, "indexguid", "varchar", "Unique identifier for organisation's elastic search index", 45),
            FieldFor<Organisation, string>(o => o.Name, "orgname", "varchar", "Name of the organisation", 500),
            FieldFor<Organisation, string>(o => o.ElasticSearchIndex, "orgindex", "varchar", "Searchable index name", 200),
            FieldFor<Organisation, DateTime>(o => o.CreateDate, "createdate", "datetime", "Date the organisation was created"),
            FieldFor<Organisation, bool>(o => o.IsReindexing, "inreindex", "tinyint", "True if organisation is currently being reindexed (may not auto-clear)"),
            FieldFor<Organisation, DateTime>(o => o.AnchorDate, "anchordate", "datetime", "Anchor date for model resets or analysis"),
            FieldFor<Organisation, int>(o => o.Quota, "quota", "int", "Monthly token quota (Table 'tagptcosts' holds usage)"),
            FieldFor<Organisation, string>(o => o.OptionsString, "optionsstring", "mediumtext", "Miscellaneous options in serialized string format", 16777215),
            FieldFor<Organisation, int>(o => o.Version, "version", "int", "Data version or schema version"),
            FieldFor<Organisation, string>(o => o.DataResidencyString, "dataresidencystring", "varchar", "Data residency (global, eu, or uk)", 60),
            FieldFor<Organisation, bool>(o => o.Mod_BE, "mod_be", "tinyint", "Whether Bid Evaluation module is enabled"),
            FieldFor<Organisation, bool>(o => o.Mod_Claude, "mod_claude", "tinyint", "Whether Claude module is enabled")
        }
    };

    private static DbField FieldFor<T, TProp>(Expression<Func<T, TProp>> expr, string dbName, string dbType, string description, int? maxLength = null)
    {
        if (expr.Body is not MemberExpression member)
            throw new ArgumentException("Expression must be a member access", nameof(expr));

        return new DbField
        {
            Name = member.Member.Name,
            DbName = dbName,
            Description = description,
            Type = typeof(TProp),
            DbType = dbType,
            MaxLength = maxLength
        };
    }
}