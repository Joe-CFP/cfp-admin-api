namespace AdminApi.DTO;

public sealed class MfaChallengeRecord
{
    public int Id { get; init; }
    public int MemberId { get; init; }
    public string MfaTokenHash { get; init; } = string.Empty;
    public DateTime CreatedUtc { get; init; }
    public DateTime ExpiresUtc { get; init; }
    public DateTime? VerifiedUtc { get; init; }
    public int FailedAttempts { get; init; }
    public DateTime? LastAttemptUtc { get; init; }
    public string? LoginIp { get; init; }
}