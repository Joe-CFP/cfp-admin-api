namespace AdminApi.DTO;

public sealed class RefreshTokenRecord
{
    public int Id { get; init; }
    public int MemberId { get; init; }
    public string TokenHash { get; init; } = string.Empty;
    public DateTime CreatedUtc { get; init; }
    public DateTime ExpiresUtc { get; init; }
    public DateTime? RevokedUtc { get; init; }
    public string? ReplacedByTokenHash { get; init; }
}