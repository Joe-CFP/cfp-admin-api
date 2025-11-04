using AdminApi.Entities;

namespace AdminApi.Repositories;

public interface IDatabaseRepository
{
    Task<IEnumerable<Organisation>> GetAllOrganisationsAsync();
    Task<IEnumerable<OrganisationSearchResult>> SearchOrganisationsAsync(string query, int limit = 10);
    Task<Member?> GetMemberByIdAsync(int id);
    Task<IEnumerable<MemberSearchResult>> SearchMembersAsync(string query, int limit = 10);
}