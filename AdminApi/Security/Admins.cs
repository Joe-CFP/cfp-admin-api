namespace AdminApi.Security;

public static class Admins
{
    private static readonly HashSet<string> Emails = new(StringComparer.OrdinalIgnoreCase)
    {
        "joe.simpson+admin@contractfinderpro.com",
    };

    public static bool IsAdmin(string email) => Emails.Contains(email);
}