namespace AdminApi.Entities;

public enum IndexKind
{
    Uk = 1,
    Global = 2
}

public enum NoticeType
{
    Tender = 1,
    Award = 2,
    Pin = 3,
}

public enum SearchField
{
    Summary = 1,
    RefTitleShort = 2,
    Publisher = 3,
    AwardedToFirstLines = 4,
    CpvDesc = 5,
    CpvCodes = 6,
    Location = 7
}

public enum QueryFieldSet
{
    Default = 1
}

public enum PublishedRange
{
    CurrentYear = 1,
    PastOneYear = 2,
    PastFiveYears = 3
}

public enum ResultOrder
{
    PublishedDateDesc = 1
}

public record SearchSpec(
    IndexKind Index,
    string? Query,
    QueryFieldSet? QueryFieldSet,
    IReadOnlyList<SearchField>? QueryFields,
    IReadOnlyList<NoticeType>? Types,
    IReadOnlyList<string>? Regions,
    DateTime? PublishedFromUtc,
    DateTime? PublishedToUtc,
    DateTime? ClosingOnOrAfterUtc,
    decimal? ValueMin,
    decimal? ValueMax,
    ResultOrder? OrderBy,
    int? PageSize
);