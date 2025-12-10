using System.Security.Cryptography;
using System.Text;
using AdminApi.Repositories;

namespace AdminApi.Services;

public static class RefreshTokenService
{
    public static async Task<string> IssueAsync(IDatabaseRepository db, int memberId, CancellationToken cancellationToken)
    {
        string token = CreateToken();
        string hash = ComputeHash(token);

        DateTime nowUtc = DateTime.UtcNow;
        DateTime expiresUtc = nowUtc.AddDays(7);

        await db.InsertRefreshTokenAsync(memberId, hash, nowUtc, expiresUtc, cancellationToken);
        return token;
    }

    public static async Task<(int MemberId, string RefreshToken)?> RotateAsync(IDatabaseRepository db, string refreshToken, CancellationToken cancellationToken)
    {
        string hash = ComputeHash(refreshToken);
        var record = await db.GetRefreshTokenByHashAsync(hash, cancellationToken);
        if (record is null) return null;
        if (record.RevokedUtc.HasValue) return null;
        if (record.ExpiresUtc <= DateTime.UtcNow) return null;

        string newToken = CreateToken();
        string newHash = ComputeHash(newToken);

        DateTime nowUtc = DateTime.UtcNow;
        DateTime expiresUtc = nowUtc.AddDays(7);

        bool rotated = await db.RotateRefreshTokenAsync(record.Id, record.TokenHash, nowUtc, newHash, record.MemberId, nowUtc, expiresUtc, cancellationToken);
        return rotated ? (record.MemberId, newToken) : null;
    }

    private static string CreateToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(64);
        string base64 = Convert.ToBase64String(bytes);
        return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private static string ComputeHash(string token)
    {
        byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
        byte[] hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}