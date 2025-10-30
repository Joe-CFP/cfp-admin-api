using System.Text.Json;
using AdminApi.Entities;
using AdminApi.Legacy;

namespace AdminApi.DTO;

public static class UserJourneyHistoryHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new LegacyDateTimeConverter() }
    };

    public static Dictionary<string, UserJourneyHistoryState> DeserializeHistory(string? historyJson)
    {
        if (string.IsNullOrWhiteSpace(historyJson))
            return new Dictionary<string, UserJourneyHistoryState>();

        try
        {
            var raw = JsonSerializer.Deserialize<Dictionary<string, RawUserJourneyHistoryState>>(historyJson, JsonOptions)
                      ?? new Dictionary<string, RawUserJourneyHistoryState>();

            var result = new Dictionary<string, UserJourneyHistoryState>(StringComparer.OrdinalIgnoreCase);

            foreach (var kv in raw)
            {
                var state = new UserJourneyHistoryState
                {
                    CurState = kv.Value.CurState,
                    CurStateDateTime = kv.Value.CurStateDateTime,
                    AutoNextState = kv.Value.AutoNextState,
                    AutoNextStateDateTime = kv.Value.AutoNextStateDateTime,
                    Actions = kv.Value.Actions != null
                        ? kv.Value.Actions.ToDictionary(p => p.Key, p => p.Value.ToAction(0))
                        : new Dictionary<string, UserJourneyAction>()
                };

                result[kv.Key] = state;
            }

            return result;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Failed to parse historyJson: {ex.Message}");
            return new Dictionary<string, UserJourneyHistoryState>();
        }
    }

    public static List<UserJourneyEvent> CalculateEvents(UserJourney journey)
    {
        List<UserJourneyEvent> events = new();

        foreach ((string stateKey, UserJourneyHistoryState stateRecord) in journey.History)
        {
            DateTime? entered = NormalizeDate(stateRecord.CurStateDateTime);
            if (entered != null)
            {
                events.Add(new UserJourneyEvent
                {
                    Timestamp = entered.Value,
                    EventType = "StateEntered",
                    State = stateKey
                });
            }

            if (stateRecord.Actions == null) continue;

            foreach (UserJourneyAction action in stateRecord.Actions.Values.Where(a => (a.WasSuccessful ?? false) || a.AttemptCount > 0))
            {
                DateTime? ts = NormalizeDate(action.CompletedAt) ?? NormalizeDate(action.ExecuteAfter);
                if (ts == null) continue;

                events.Add(new UserJourneyEvent
                {
                    Timestamp = ts.Value,
                    EventType = "ActionExecuted",
                    State = stateKey,
                    ActionName = action.ActionName,
                    WasSuccessful = action.WasSuccessful,
                    IsPending = false
                });
            }
        }

        if (NormalizeDate(journey.CurrentStateDateTime) is { } curEntered)
        {
            events.Add(new UserJourneyEvent
            {
                Timestamp = curEntered,
                EventType = "StateEntered",
                State = journey.CurrentState
            });
        }

        foreach (var action in journey.Actions)
        {
            DateTime? timeStamp = NormalizeDate(action.CompletedAt) ?? NormalizeDate(action.ExecuteAfter);
            if (timeStamp == null) continue;

            events.Add(new UserJourneyEvent
            {
                Timestamp = timeStamp.Value,
                EventType = "ActionExecuted",
                State = journey.CurrentState,
                ActionName = action.ActionName,
                WasSuccessful = action.WasSuccessful,
                IsPending = action is { WasSuccessful: null }
            });
        }

        return events.OrderBy(e => e.Timestamp).ToList();
    }

    private static DateTime? NormalizeDate(DateTime? date)
    {
        if (date == null) return null;
        if (date == DateTime.MinValue) return null;
        if (date.Value.Year < 1900) return null;
        return date;
    }
}

public class UserJourneyHistoryState
{
    public string? CurState { get; set; }
    public DateTime? CurStateDateTime { get; set; }
    public string? AutoNextState { get; set; }
    public DateTime? AutoNextStateDateTime { get; set; }
    public Dictionary<string, UserJourneyAction>? Actions { get; set; }
}

public class RawUserJourneyHistoryState
{
    public string? CurState { get; set; }
    public DateTime? CurStateDateTime { get; set; }
    public string? AutoNextState { get; set; }
    public DateTime? AutoNextStateDateTime { get; set; }
    public Dictionary<string, UserJourneyRecord.UserJourneyActionRecord>? Actions { get; set; }
}
