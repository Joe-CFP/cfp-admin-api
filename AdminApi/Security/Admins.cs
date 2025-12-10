namespace AdminApi.Security;

public static class Admins
{
    private static readonly HashSet<string> Usernames = new(StringComparer.OrdinalIgnoreCase)
    {
        "joe.simpson+admin@contractfinderpro.com",
    };

    public static bool IsAdmin(string email) => Usernames.Contains(email);
}