namespace AdminApi.DTO;

public sealed class MemberSecurityRecord
{
    public string Username { get; init; } = string.Empty;
    public string HashedPassword { get; init; } = string.Empty;
    public DateTime LoginExpiry { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}