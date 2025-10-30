namespace AdminApi.Entities;

public class Member: MemberRecord
{
    public UserJourney? UserJourney { get; set; }
}

public class MemberSearchResult
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}
