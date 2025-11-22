// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using AdminApi.Entities;
using AdminApi.Legacy;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace AdminApi.DTO;

public class UserJourneyRecordBase
{
    public int UserJourneyId { get; set; }
    public string Username { get; set; }
    public string CurrentState { get; set; }
    public DateTime CurrentStateDateTime { get; set; }
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
        UserJourney journey = new();
        PropertyMapper.CopyMatchingProperties(this, journey);

        var actions = new List<UserJourneyAction>();
        for (int i = 1; i <= 20; i++)
        {
            string? raw = (string?)GetType().GetProperty($"Action{i}")?.GetValue(this);
            if (string.IsNullOrWhiteSpace(raw)) continue;

            try
            {
                JsonSerializerOptions options = new() {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new LegacyDateTimeConverter() }
                };
                UserJourneyActionRecord? parsed = JsonSerializer.Deserialize<UserJourneyActionRecord>(raw, options);
                if (parsed != null)
                    actions.Add(parsed.ToAction(i));
            }
            catch(JsonException exception)
            {
                // Ignore bad data
                Console.WriteLine($"Invalid UserJourney action JSON: {exception.Message}\nValue: {raw}");
            }
        }

        journey.Actions = actions;
        journey.History = UserJourneyHistoryHelper.DeserializeHistory(HistoryJson);
        journey.Events = UserJourneyHistoryHelper.CalculateEvents(journey);
        return journey;
    }
    
    public class UserJourneyActionRecord
    {
        public DateTime CompletedAt { get; set; }
        public DateTime ExecuteAfter { get; set; }
        public int TryCount { get; set; }
        public bool Success { get; set; }
        public int Action { get; set; }
        public string ActionString { get; set; }

        public UserJourneyAction ToAction(int index)
        {
            return new() {
                ActionIndex = index,
                ActionId = Action,
                ActionName = ActionString,
                ActionAbbreviation = AbbreviationForActionName(ActionString),
                AttemptCount = TryCount,
                WasSuccessful = TryCount > 0 ? Success : null,
                WasAttempted = TryCount > 0,
                IsScheduled = TryCount == 0 && ExecuteAfter > DateTime.UtcNow,
                CompletedAt = CompletedAt == DateTime.MinValue ? null : CompletedAt,
                ExecuteAfter = ExecuteAfter == DateTime.MinValue ? null : ExecuteAfter
            };
        }
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public static string? AbbreviationForActionName(string actionName)
    {
        return actionName.ToLowerInvariant() switch
        {
            "emailuserconfirm_101" => "euc_101",
            "emailftdetails_201" => "efd_201",
            "emailliketosubscribe_202" => "els_202",
            "emailsetupsearch_203" => "ess_203",
            "emailthanksforsubscribing_301" => "ets_301",
            "emailtrialabouttoexpire_302" => "etae_302",
            "emailtrialabouttoexpire_304" => "etae_304",
            "emailwelcometocfp_401" => "ewc_401",
            "emailwelcometocfpdirect_402" => "ewcd_402",
            "emailliketrialextended_501" => "elte_501",
            "emailseeifinterested_601" => "esi_601",
            "emailcancelsubscription_701" => "ecs_701",
            "emailcancelsubscriptionstilltrial_702" => "ecs_702",
            "emaillosingaccess_703" => "ela_703",
            "emailcancelsubscriptionpastdue_704" => "ecsp_704",
            "emailvideotour_305" => "evt_305",
            "emailyourmissingout_306" => "eym_306",
            "extendaccess" => "extacc",
            "checkuserverified" => "cuv",
            _ => null,
        };
    }
}