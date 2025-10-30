using AdminApi.DTO;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace AdminApi.Entities;

public class UserJourney : UserJourneyRecordBase
{
    public List<UserJourneyAction> Actions { get; set; } = new();
    public Dictionary<string, UserJourneyHistoryState> History { get; set; }
    public List<UserJourneyEvent> Events { get; set; }
}

public class UserJourneyAction
{
    public int ActionIndex { get; set; }
    public int ActionId { get; set; }
    public string ActionName { get; set; }
    public string? ActionAbbreviation { get; set; }
    public int AttemptCount { get; set; }
    public bool WasAttempted { get; set; }
    public bool? WasSuccessful { get; set; }
    public bool IsScheduled { get; set; }
    public DateTime? ExecuteAfter { get; set; }
    public DateTime? CompletedAt { get; set; }
}
