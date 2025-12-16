using AdminApi.DTO;
using Dapper;
using MySql.Data.MySqlClient;

namespace AdminApi.Repositories;

public partial class DatabaseRepository
{
    public async Task<MemberSecurityRecord?> GetMemberSecurityRecordByUsernameAsync(string username)
    {
        await using MySqlConnection connection = new(ConnectionString);

        const string sql =
            "SELECT m.id Id, m.username Username, m.password HashedPassword, " +
            "m.login_expiry LoginExpiry, m.firstname FirstName, m.lastname LastName, " +
            "m.isadmin IsAdmin, m.totpsecret TotpSecret " +
            "FROM members m WHERE m.username = @Username LIMIT 2";

        MemberSecurityRecord[] members =
            (await connection.QueryAsync<MemberSecurityRecord>(sql, new { Username = username })).ToArray();

        return members.Length != 1 ? null : members[0];
    }

    public async Task<MemberSecurityRecord?> GetMemberSecurityRecordByIdAsync(int memberId)
    {
        await using MySqlConnection connection = new(ConnectionString);

        const string sql =
            "SELECT m.id Id, m.username Username, m.password HashedPassword, " +
            "m.login_expiry LoginExpiry, m.firstname FirstName, m.lastname LastName, " +
            "m.isadmin IsAdmin, m.totpsecret TotpSecret " +
            "FROM members m WHERE m.id = @MemberId LIMIT 1";

        return await connection.QueryFirstOrDefaultAsync<MemberSecurityRecord>(sql, new { MemberId = memberId });
    }

    public async Task InsertLoginHistoryAsync(int memberId, string loginIp, string loginUrl, string referCode,
        CancellationToken cancellationToken)
    {
        await using MySqlConnection connection = new(ConnectionString);

        const string sql =
            "INSERT INTO loginhistory (memberid, logindate, refercode, loginurl, loginip) " +
            "VALUES (@MemberId, CURRENT_TIMESTAMP, @ReferCode, @LoginUrl, @LoginIp)";

        CommandDefinition cmd = new(sql,
            new { MemberId = memberId, ReferCode = referCode, LoginUrl = loginUrl, LoginIp = loginIp, },
            cancellationToken: cancellationToken
        );

        await connection.ExecuteAsync(cmd);
    }

    public async Task InsertRefreshTokenAsync(int memberId, string tokenHash, DateTime createdUtc, DateTime expiresUtc, CancellationToken cancellationToken)
    {
        await using MySqlConnection connection = new(ConnectionString);

        const string sql =
            "INSERT INTO refreshtokens (memberid, tokenhash, created_utc, expires_utc, revoked_utc, replacedbytokenhash) " +
            "VALUES (@MemberId, @TokenHash, @CreatedUtc, @ExpiresUtc, NULL, NULL)";

        CommandDefinition cmd = new(sql,
            new { MemberId = memberId, TokenHash = tokenHash, CreatedUtc = createdUtc, ExpiresUtc = expiresUtc },
            cancellationToken: cancellationToken
        );

        await connection.ExecuteAsync(cmd);
    }

    public async Task<RefreshTokenRecord?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        await using MySqlConnection connection = new(ConnectionString);

        const string sql =
            "SELECT id Id, memberid MemberId, tokenhash TokenHash, created_utc CreatedUtc, expires_utc ExpiresUtc, revoked_utc RevokedUtc, replacedbytokenhash ReplacedByTokenHash " +
            "FROM refreshtokens WHERE tokenhash = @TokenHash LIMIT 1";

        CommandDefinition cmd = new(sql, new { TokenHash = tokenHash }, cancellationToken: cancellationToken);
        return await connection.QueryFirstOrDefaultAsync<RefreshTokenRecord>(cmd);
    }

    public async Task<bool> RotateRefreshTokenAsync(int refreshTokenId, string tokenHash, DateTime revokedUtc, string newTokenHash, int memberId, DateTime createdUtc, DateTime expiresUtc, CancellationToken cancellationToken)
    {
        await using MySqlConnection connection = new(ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using MySqlTransaction tx = connection.BeginTransaction();

        const string updateSql =
            "UPDATE refreshtokens " +
            "SET revoked_utc = @RevokedUtc, replacedbytokenhash = @NewTokenHash " +
            "WHERE id = @Id AND tokenhash = @TokenHash AND revoked_utc IS NULL";

        CommandDefinition updateCmd = new(updateSql,
            new { Id = refreshTokenId, TokenHash = tokenHash, RevokedUtc = revokedUtc, NewTokenHash = newTokenHash },
            transaction: tx,
            cancellationToken: cancellationToken
        );

        int updated = await connection.ExecuteAsync(updateCmd);
        if (updated != 1)
        {
            tx.Rollback();
            return false;
        }

        const string insertSql =
            "INSERT INTO refreshtokens (memberid, tokenhash, created_utc, expires_utc, revoked_utc, replacedbytokenhash) " +
            "VALUES (@MemberId, @TokenHash, @CreatedUtc, @ExpiresUtc, NULL, NULL)";

        CommandDefinition insertCmd = new(insertSql,
            new { MemberId = memberId, TokenHash = newTokenHash, CreatedUtc = createdUtc, ExpiresUtc = expiresUtc },
            transaction: tx,
            cancellationToken: cancellationToken
        );

        await connection.ExecuteAsync(insertCmd);

        tx.Commit();
        return true;
    }

    public async Task InsertMfaChallengeAsync(int memberId, string mfaTokenHash, DateTime createdUtc, DateTime expiresUtc, string loginIp, CancellationToken cancellationToken)
    {
        await using MySqlConnection connection = new(ConnectionString);

        const string sql =
            "INSERT INTO mfachallenges (memberid, mfatokenhash, created_utc, expires_utc, verified_utc, failedattempts, lastattempt_utc, loginip) " +
            "VALUES (@MemberId, @MfaTokenHash, @CreatedUtc, @ExpiresUtc, NULL, 0, NULL, @LoginIp)";

        CommandDefinition cmd = new(sql,
            new { MemberId = memberId, MfaTokenHash = mfaTokenHash, CreatedUtc = createdUtc, ExpiresUtc = expiresUtc, LoginIp = loginIp },
            cancellationToken: cancellationToken
        );

        await connection.ExecuteAsync(cmd);
    }

    public async Task<MfaChallengeRecord?> GetMfaChallengeByHashAsync(string mfaTokenHash, CancellationToken cancellationToken)
    {
        await using MySqlConnection connection = new(ConnectionString);

        const string sql =
            "SELECT id Id, memberid MemberId, mfatokenhash MfaTokenHash, created_utc CreatedUtc, expires_utc ExpiresUtc, verified_utc VerifiedUtc, failedattempts FailedAttempts, lastattempt_utc LastAttemptUtc, loginip LoginIp " +
            "FROM mfachallenges WHERE mfatokenhash = @MfaTokenHash LIMIT 1";

        CommandDefinition cmd = new(sql, new { MfaTokenHash = mfaTokenHash }, cancellationToken: cancellationToken);
        return await connection.QueryFirstOrDefaultAsync<MfaChallengeRecord>(cmd);
    }

    public async Task<bool> IncrementMfaChallengeFailedAttemptsAsync(int id, DateTime lastAttemptUtc, CancellationToken cancellationToken)
    {
        await using MySqlConnection connection = new(ConnectionString);

        const string sql =
            "UPDATE mfachallenges " +
            "SET failedattempts = failedattempts + 1, lastattempt_utc = @LastAttemptUtc " +
            "WHERE id = @Id AND verified_utc IS NULL";

        CommandDefinition cmd = new(sql, new { Id = id, LastAttemptUtc = lastAttemptUtc }, cancellationToken: cancellationToken);
        int updated = await connection.ExecuteAsync(cmd);
        return updated == 1;
    }

    public async Task<bool> MarkMfaChallengeVerifiedAsync(int id, DateTime verifiedUtc, CancellationToken cancellationToken)
    {
        await using MySqlConnection connection = new(ConnectionString);

        const string sql =
            "UPDATE mfachallenges " +
            "SET verified_utc = @VerifiedUtc " +
            "WHERE id = @Id AND verified_utc IS NULL";

        CommandDefinition cmd = new(sql, new { Id = id, VerifiedUtc = verifiedUtc }, cancellationToken: cancellationToken);
        int updated = await connection.ExecuteAsync(cmd);
        return updated == 1;
    }
}
