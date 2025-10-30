namespace AdminApi.DTO;

public class UserJourneyEvent
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } // "StateEntered", "ActionExecuted"
    public string State { get; set; }
    public string? ActionName { get; set; }
    public bool? WasSuccessful { get; set; }
    public bool IsPending { get; set; }
}