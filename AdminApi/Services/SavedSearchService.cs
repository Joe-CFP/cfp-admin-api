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
            Task<long> curTask = CountVariantAsync(s, SavedSearchCountVariant.Current, ct);
            Task<long> lastTask = CountVariantAsync(s, SavedSearchCountVariant.PastOneYear, ct);
            Task<long> fiveTask = CountVariantAsync(s, SavedSearchCountVariant.PastFiveYears, ct);

            await Task.WhenAll(curTask, lastTask, fiveTask);

            s.CurrentTotal = curTask.Result;
            s.LastYearTotal = lastTask.Result;
            s.FiveYearTotal = fiveTask.Result;
        });
        await Task.WhenAll(tasks);
        return items;
    }

    private enum SavedSearchCountVariant
    {
        Current = 1,
        PastOneYear = 2,
        PastFiveYears = 3
    }

    private Task<long> CountVariantAsync(SavedSearch saved, SavedSearchCountVariant variant, CancellationToken ct)
    {
        DateTime now = DateTime.UtcNow;

        (DateTime? fromUtc, DateTime? toUtc) range = variant switch {
            SavedSearchCountVariant.Current => (null, null),
            SavedSearchCountVariant.PastOneYear => (now.AddYears(-1), now),
            SavedSearchCountVariant.PastFiveYears => (now.AddYears(-5), now),
            _ => (null, null)
        };

        SearchSpec spec = saved.Spec with {
            PublishedFromUtc = range.fromUtc,
            PublishedToUtc = range.toUtc,
            ClosingOnOrAfterUtc = ShouldExcludeCurrentlyClosed(variant) ? now.Date : null
        };

        return os.CountAsync(spec, ct);
    }

    private static bool ShouldExcludeCurrentlyClosed(SavedSearchCountVariant variant)
        => variant == SavedSearchCountVariant.Current;
}
