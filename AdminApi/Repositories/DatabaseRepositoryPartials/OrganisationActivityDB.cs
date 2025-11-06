using AdminApi.Entities;
using MySql.Data.MySqlClient;

namespace AdminApi.Repositories;

public partial class DatabaseRepository
{
    public async Task<MemberActivity> GetOrganisationMemberActivityAsync(int orgId)
    {
        await using MySqlConnection connection = new(ConnectionString);

        const string loginSql = """
                                SELECT DISTINCT DATE(lh.logindate) AS Day
                                FROM loginhistory lh
                                JOIN members m ON m.id = lh.memberid
                                JOIN kborgindex o ON o.orgguid = m.orgguid
                                WHERE o.orgid = @orgId
                                  AND lh.logindate >= DATE_SUB(CURDATE(), INTERVAL 90 DAY)
                                ORDER BY Day DESC
                                """;

        const string emailSql = """
                                SELECT DISTINCT DATE(eh.sentdate) AS Day
                                FROM emailhistory eh
                                JOIN members m ON m.email = eh.destemail
                                JOIN kborgindex o ON o.orgguid = m.orgguid
                                WHERE o.orgid = @orgId
                                  AND eh.sent = 1
                                  AND eh.sentdate >= DATE_SUB(CURDATE(), INTERVAL 90 DAY)
                                ORDER BY Day DESC
                                """;

        const string errorSql = """
                                SELECT DISTINCT DATE(eh.sentdate) AS Day
                                FROM emailhistory eh
                                JOIN members m ON m.email = eh.destemail
                                JOIN kborgindex o ON o.orgguid = m.orgguid
                                WHERE o.orgid = @orgId
                                  AND (eh.sent = 0 OR NULLIF(TRIM(eh.errortext), '') IS NOT NULL)
                                  AND eh.sentdate >= DATE_SUB(CURDATE(), INTERVAL 90 DAY)
                                ORDER BY Day DESC
                                """;

        const string ninjaSql = """
                                SELECT DISTINCT DATE(c.createdate) AS Day
                                FROM tagptcosts c
                                WHERE c.orgid = @orgId
                                  AND c.createdate >= DATE_SUB(CURDATE(), INTERVAL 90 DAY)
                                ORDER BY Day DESC
                                """;

        List<string> loginDays = await GetStringDatesFromDatabase(connection, loginSql, new { orgId });
        List<string> emailDays = await GetStringDatesFromDatabase(connection, emailSql, new { orgId });
        List<string> emailErrorDays = await GetStringDatesFromDatabase(connection, errorSql, new { orgId });
        List<string> tenderNinjaDays = await GetStringDatesFromDatabase(connection, ninjaSql, new { orgId });

        return new MemberActivity
        {
            LoginDays = loginDays,
            EmailDays = emailDays,
            EmailErrorDays = emailErrorDays,
            TenderNinjaDays = tenderNinjaDays
        };
    }
}
