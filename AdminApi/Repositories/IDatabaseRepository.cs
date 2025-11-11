using AdminApi.Entities;

namespace AdminApi.Repositories;

public interface IDatabaseRepository
{
    Task<Member?> GetMemberByIdAsync(int id);
    Task<IEnumerable<MemberPreview>> SearchMembersAsync(string query, int limit = 10);
    Task<List<OrganisationSearchResult>> GetAllOrganisationSummariesAsync();
    Task<Organisation> GetOrganisationByIdAsync(int id);
    Task<MemberPreview?> GetMemberPreviewByEmailAsync(string email);
}