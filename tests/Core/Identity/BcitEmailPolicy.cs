namespace BCIT_Social.Core.Identity;

/// <summary>
/// "BCIT only" membership rule, shared by Register and Login (both pages had a copy of the same EndsWith check).
/// Stricter than the originals: non-empty local part, exactly one '@', exact domain match.
/// </summary>
public static class BcitEmailPolicy
{
    private static readonly string[] AllowedDomains = ["bcit.ca", "my.bcit.ca"];

    public static bool IsBcitEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        var value = email.Trim();
        var at = value.IndexOf('@');
        if (at <= 0 || at != value.LastIndexOf('@')) return false;

        var domain = value[(at + 1)..];
        return AllowedDomains.Contains(domain, StringComparer.OrdinalIgnoreCase);
    }
}
