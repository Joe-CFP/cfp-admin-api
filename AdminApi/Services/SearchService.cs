using AdminApi.Entities;
using AdminApi.Repositories;

namespace AdminApi.Services;

public interface ISearchService
{
    SearchBuilder UseIndex(IndexKind index);
}

public class SearchService(IOpenSearchRepository os) : ISearchService
{
    public SearchBuilder UseIndex(IndexKind index) => new(os, index);
}

public class SearchBuilder
{
    private readonly IOpenSearchRepository _os;
    private readonly IndexKind _index;

    private bool _searchForSet;
    private string? _term;
    private QueryFieldSet? _termFieldSet;
    private IReadOnlyList<SearchField>? _termFields;

    private bool _typeSet;
    private NoticeType[]? _types;

    private bool _regionSet;
    private string[]? _regions;

    private bool _publishedRangeSet;
    private PublishedRange? _publishedRange;

    private bool _excludeCurrentlyClosedSet;
    private bool _excludeCurrentlyClosed;

    private bool _valueSet;
    private decimal? _valueMin;
    private decimal? _valueMax;

    private bool _orderSet;
    private ResultOrder? _orderBy;

    private bool _pageSizeSet;
    private int? _pageSize;

    public SearchBuilder(IOpenSearchRepository os, IndexKind index)
    {
        _os = os;
        _index = index;
    }

    public SearchBuilder SearchFor(string term)
    {
        return SearchFor(term, QueryFieldSet.Default);
    }

    public SearchBuilder SearchFor(string term, QueryFieldSet fieldSet)
    {
        if (_searchForSet) throw new InvalidOperationException("SearchFor can only be called once.");
        if (string.IsNullOrWhiteSpace(term)) throw new ArgumentException("Term is required.", nameof(term));
        _searchForSet = true;
        _term = term;
        _termFieldSet = fieldSet;
        _termFields = null;
        return this;
    }

    public SearchBuilder SearchFor(string term, params SearchField[] fields)
    {
        if (_searchForSet) throw new InvalidOperationException("SearchFor can only be called once.");
        if (string.IsNullOrWhiteSpace(term)) throw new ArgumentException("Term is required.", nameof(term));
        if (fields == null || fields.Length == 0) throw new ArgumentException("At least one field is required.", nameof(fields));
        _searchForSet = true;
        _term = term;
        _termFieldSet = null;
        _termFields = fields.Distinct().ToArray();
        return this;
    }

    public SearchBuilder FilterByType(params NoticeType[] types)
    {
        if (_typeSet) throw new InvalidOperationException("FilterByType can only be called once.");
        if (types == null || types.Length == 0) throw new ArgumentException("At least one type is required.", nameof(types));
        _typeSet = true;
        _types = types;
        return this;
    }

    public SearchBuilder FilterByRegion(params string[] regions)
    {
        if (_regionSet) throw new InvalidOperationException("FilterByRegion can only be called once.");
        if (regions == null || regions.Length == 0) throw new ArgumentException("At least one region is required.", nameof(regions));
        _regionSet = true;
        _regions = regions.Where(r => !string.IsNullOrWhiteSpace(r)).ToArray();
        return this;
    }

    public SearchBuilder FilterByPublishedRange(PublishedRange range)
    {
        if (_publishedRangeSet) throw new InvalidOperationException("FilterByPublishedRange can only be called once.");
        _publishedRangeSet = true;
        _publishedRange = range;
        return this;
    }

    public SearchBuilder ExcludeCurrentlyClosed()
    {
        if (_excludeCurrentlyClosedSet) throw new InvalidOperationException("ExcludeCurrentlyClosed can only be called once.");
        _excludeCurrentlyClosedSet = true;
        _excludeCurrentlyClosed = true;
        return this;
    }

    public SearchBuilder FilterByValue(decimal? min, decimal? max)
    {
        if (_valueSet) throw new InvalidOperationException("FilterByValue can only be called once.");
        if (min == null && max == null) throw new ArgumentException("At least one of min/max must be provided.");
        _valueSet = true;
        _valueMin = min;
        _valueMax = max;
        return this;
    }

    public SearchBuilder OrderBy(ResultOrder order)
    {
        if (_orderSet) throw new InvalidOperationException("OrderBy can only be called once.");
        _orderSet = true;
        _orderBy = order;
        return this;
    }

    public SearchBuilder SetPageSize(int pageSize)
    {
        if (_pageSizeSet) throw new InvalidOperationException("SetPageSize can only be called once.");
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be > 0.");
        _pageSizeSet = true;
        _pageSize = pageSize;
        return this;
    }

    public Task<long> CountAsync(CancellationToken ct = default)
    {
        SearchSpec spec = BuildSpecForCount();
        return _os.CountAsync(spec, ct);
    }

    public Task<object> FetchPageAsync(int pageNumber, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<object> FetchAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    private SearchSpec BuildSpecForCount()
    {
        DateTime now = DateTime.UtcNow;
        DateTime? publishedFrom = null;
        DateTime? publishedTo = null;

        if (_publishedRange is not null)
        {
            publishedTo = now;
            publishedFrom = _publishedRange.Value switch {
                PublishedRange.CurrentYear => new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                PublishedRange.PastOneYear => now.AddYears(-1),
                PublishedRange.PastFiveYears => now.AddYears(-5),
                _ => null
            };
        }

        DateTime? closingOnOrAfter = _excludeCurrentlyClosed ? now.Date : null;

        return new SearchSpec(
            Index: _index,
            Query: _term,
            QueryFieldSet: _termFieldSet,
            QueryFields: _termFields,
            Types: _types,
            Regions: _regions,
            PublishedFromUtc: publishedFrom,
            PublishedToUtc: publishedTo,
            ClosingOnOrAfterUtc: closingOnOrAfter,
            ValueMin: _valueMin,
            ValueMax: _valueMax,
            OrderBy: _orderBy,
            PageSize: _pageSize
        );
    }
}
