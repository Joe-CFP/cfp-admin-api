using AdminApi.DTO;

namespace AdminApi.Entities;

public class UserJourney : UserJourneyRecordBase
{
    public List<string> Actions { get; set; } = new();
}