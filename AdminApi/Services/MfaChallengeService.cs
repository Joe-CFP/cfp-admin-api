using System.Security.Cryptography;
using System.Text;
using AdminApi.Repositories;

namespace AdminApi.Services;

public static class MfaChallengeService
{
    public static async Task<(string Token, DateTimeOffset ExpiresUtc)> CreateAsync(IDatabaseRepository db, int memberId, string loginIp, CancellationToken cancellationToken)
    {
        string token = CreateToken();
        string hash = ComputeHash(token);

        DateTime nowUtc = DateTime.UtcNow;
        DateTime expiresUtc = nowUtc.AddMinutes(5);

        await db.InsertMfaChallengeAsync(memberId, hash, nowUtc, expiresUtc, loginIp, cancellationToken);

        return (token, new DateTimeOffset(expiresUtc, TimeSpan.Zero));
    }

    public static string ComputeHash(string token) => ComputeHashInternal(token);

    private static string CreateToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(48);
        string base64 = Convert.ToBase64String(bytes);
        return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private static string ComputeHashInternal(string token)
    {
        byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
        byte[] hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}