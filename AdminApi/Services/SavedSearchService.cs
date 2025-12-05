using AdminApi.Entities;
using AdminApi.Repositories;

namespace AdminApi.Services;

public interface ISavedSearchService
{
    Task<List<SavedSearch>> GetSavedSearchesByMemberIdAsync(int memberId, CancellationToken ct = default);
}

public class SavedSearchService(IDatabaseRepository db, IOpenSearchRepository os) : ISavedSearchService
{
    public async Task<List<SavedSearch>> GetSavedSearchesByMemberIdAsync(int memberId, CancellationToken ct = default)
    {
        List<SavedSearch> items = (await db.GetSavedSearchesByMemberIdAsync(memberId)).ToList();
        IEnumerable<Task> tasks = items.Select(async s =>
        {
            (long cur, long last, long five) = await os.GetCountsAsync(s, ct);
            s.CurrentTotal = cur;
            s.LastYearTotal = last;
            s.FiveYearTotal = five;
        });
        await Task.WhenAll(tasks);
        return items;
    }
}