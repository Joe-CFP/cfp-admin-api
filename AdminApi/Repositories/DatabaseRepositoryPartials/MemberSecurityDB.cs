using AdminApi.DTO;
using Dapper;
using MySql.Data.MySqlClient;

namespace AdminApi.Repositories;

public partial class DatabaseRepository
{
    public async Task<MemberSecurityRecord?> GetMemberSecurityRecordByEmailAsync(string email)
    {
        await using MySqlConnection connection = new(ConnectionString);

        const string sql =
            "SELECT m.username Username, m.password HashedPassword, m.login_expiry LoginExpiry " +
            "FROM members m WHERE m.email = @Email LIMIT 2";

        MemberSecurityRecord[] members = (await connection.QueryAsync<MemberSecurityRecord>(sql, new { Email = email })).ToArray();
        if (members.Length != 1) return null;

        MemberSecurityRecord member = members[0];
        if (!string.Equals(member.Username, email, StringComparison.OrdinalIgnoreCase)) return null;

        return member;
    }
}
