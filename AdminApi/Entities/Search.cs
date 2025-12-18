namespace AdminApi.Entities;

public enum IndexKind
{
    Uk = 1,
    Global = 2
}

public enum NoticeType
{
    Pin = 1,
    Tender = 2,
    Award = 3
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

public enum SearchFieldSet
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
    string? Term,
    SearchFieldSet? TermFieldSet,
    IReadOnlyList<SearchField>? TermFields,
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