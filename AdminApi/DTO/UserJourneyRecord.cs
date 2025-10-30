// ReSharper disable UnusedAutoPropertyAccessor.Global

using AdminApi.Entities;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace AdminApi.DTO;

public class UserJourneyRecordBase
{
    public int UjId { get; set; }
    public string Username { get; set; }
    public string CurState { get; set; }
    public DateTime CurStateDateTime { get; set; }
    public string? AutoNextState { get; set; }
    public DateTime? AutoNextStateDateTime { get; set; }
    public string? Summary { get; set; }
    public string? HistoryJson { get; set; }
}

public class UserJourneyRecord : UserJourneyRecordBase
{
    public string? Action1 { get; set; }
    public string? Action2 { get; set; }
    public string? Action3 { get; set; }
    public string? Action4 { get; set; }
    public string? Action5 { get; set; }
    public string? Action6 { get; set; }
    public string? Action7 { get; set; }
    public string? Action8 { get; set; }
    public string? Action9 { get; set; }
    public string? Action10 { get; set; }
    public string? Action11 { get; set; }
    public string? Action12 { get; set; }
    public string? Action13 { get; set; }
    public string? Action14 { get; set; }
    public string? Action15 { get; set; }
    public string? Action16 { get; set; }
    public string? Action17 { get; set; }
    public string? Action18 { get; set; }
    public string? Action19 { get; set; }
    public string? Action20 { get; set; }

    public UserJourney ToUserJourney()
    {
        UserJourney journey = new UserJourney();
        PropertyMapper.CopyMatchingProperties(this, journey);

        var actions = new List<string?>();
        for (int i = 1; i <= 20; i++)
        {
            string? value = (string?)GetType().GetProperty($"Action{i}")?.GetValue(this);
            if (!string.IsNullOrWhiteSpace(value))
                actions.Add(value);
        }

        journey.Actions = actions;
        return journey;
    }
}
