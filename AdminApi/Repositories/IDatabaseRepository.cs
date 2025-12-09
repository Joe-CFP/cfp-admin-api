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
    Task<MemberSecurityRecord?> GetMemberSecurityRecordByEmailAsync(string email);
}