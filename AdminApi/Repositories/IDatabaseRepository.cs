using AdminApi.DTO;
using AdminApi.Entities;

namespace AdminApi.Repositories;

public interface IDatabaseRepository
{
    Task<Member?> GetMemberByIdAsync(int id);
    Task<UserJourney?> GetMemberJourneyByIdAsync(int id);
    Task<MemberActivity?> GetMemberActivityByIdAsync(int id);
    Task<IEnumerable<MemberPreview>> SearchMembersAsync(string query, int limit = 10);
    Task<List<OrganisationPreview>> GetAllOrganisationPreviewsAsync();
    Task<Organisation?> GetOrganisationByIdAsync(int id);
    Task<MemberActivity> GetOrganisationMemberActivityAsync(int orgId);
    Task<MemberPreview?> GetMemberPreviewByEmailAsync(string email);
    Task<IEnumerable<SavedSearch>> GetSavedSearchesByMemberIdAsync(int id);

    Task<MemberSecurityRecord?> GetMemberSecurityRecordByUsernameAsync(string username);
    Task<MemberSecurityRecord?> GetMemberSecurityRecordByIdAsync(int memberId);
    Task InsertLoginHistoryAsync(int memberId, string loginIp, string loginUrl, string referCode, CancellationToken cancellationToken);

    Task InsertRefreshTokenAsync(int memberId, string tokenHash, DateTime createdUtc, DateTime expiresUtc, CancellationToken cancellationToken);
    Task<RefreshTokenRecord?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task<bool> RotateRefreshTokenAsync(int refreshTokenId, string tokenHash, DateTime revokedUtc, string newTokenHash, int memberId, DateTime createdUtc, DateTime expiresUtc, CancellationToken cancellationToken);

    Task InsertMfaChallengeAsync(int memberId, string mfaTokenHash, DateTime createdUtc, DateTime expiresUtc, string loginIp, CancellationToken cancellationToken);
    Task<MfaChallengeRecord?> GetMfaChallengeByHashAsync(string mfaTokenHash, CancellationToken cancellationToken);
    Task<bool> IncrementMfaChallengeFailedAttemptsAsync(int id, DateTime lastAttemptUtc, CancellationToken cancellationToken);
    Task<bool> MarkMfaChallengeVerifiedAsync(int id, DateTime verifiedUtc, CancellationToken cancellationToken);
}
