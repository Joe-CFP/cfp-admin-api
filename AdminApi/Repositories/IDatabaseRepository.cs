using AdminApi.Entities;

namespace AdminApi.Repositories;

public interface IDatabaseRepository
{
    Task<IEnumerable<Organisation>> GetAllOrganisationsAsync();
    Task<Member> GetMemberByIdAsync(int id);
    Task<IEnumerable<MemberSearchResult>> SearchMembersAsync(string query, int limit = 10);
    Task<List<OrganisationSearchResult>> GetAllOrganisationSummariesAsync();
    Task<Organisation> GetOrganisationByIdAsync(int id);
}