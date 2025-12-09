namespace AdminApi.Security;

public static class Admins
{
    private static readonly HashSet<string> Emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "replace-with-real-admin@example.com"
    };

    public static bool IsAllowed(string email) => Emails.Contains(email);
}