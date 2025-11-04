using AdminApi.Entities;
using Dapper;
using MySql.Data.MySqlClient;

namespace AdminApi.Repositories;

public partial class DatabaseRepository
{
    private async Task<MemberActivity> GetMemberActivityAsync(int memberId, string email)
    {
        await using MySqlConnection connection = new(ConnectionString);

        const string loginSql = """
                                SELECT DISTINCT DATE(logindate) AS Day
                                FROM loginhistory
                                WHERE memberid = @memberId
                                  AND logindate >= DATE_SUB(CURDATE(), INTERVAL 90 DAY)
                                ORDER BY Day DESC
                                """;

        const string emailSql = """
                                SELECT DISTINCT DATE(sentdate) AS Day
                                FROM emailhistory
                                WHERE destemail = @email
                                  AND sent = 1
                                  AND sentdate >= DATE_SUB(CURDATE(), INTERVAL 90 DAY)
                                ORDER BY Day DESC
                                """;

        const string errorSql = """
                                SELECT DISTINCT DATE(sentdate) AS Day
                                FROM emailhistory
                                WHERE destemail = @email
                                  AND (sent = 0 OR NULLIF(TRIM(errortext), '') IS NOT NULL)
                                  AND sentdate >= DATE_SUB(CURDATE(), INTERVAL 90 DAY)
                                ORDER BY Day DESC
                                """;

        IEnumerable<DateTime> loginDays = await connection.QueryAsync<DateTime>(loginSql, new { memberId });
        IEnumerable<DateTime> emailDays = await connection.QueryAsync<DateTime>(emailSql, new { email });
        IEnumerable<DateTime> emailErrorDays = await connection.QueryAsync<DateTime>(errorSql, new { email });

        return new MemberActivity
        {
            LoginDays = loginDays.Select(d => d.ToString("yyyy-MM-dd")).ToList(),
            EmailDays = emailDays.Select(d => d.ToString("yyyy-MM-dd")).ToList(),
            EmailErrorDays = emailErrorDays.Select(d => d.ToString("yyyy-MM-dd")).ToList()
        };
    }
}
