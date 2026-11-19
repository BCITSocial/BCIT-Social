namespace BCIT_Social.Core.Profiles;

/// <summary>
/// "Profile display name → user name → fallback" appeared in at least seven places
/// (Post details, Home cards, comments, Messages, Profile friends, requests, Community members) with four different fallbacks.
/// </summary>
public static class DisplayNameResolver
{
    public static string Resolve(string fallback, params string?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate)) return candidate.Trim();
        }

        return fallback;
    }
}
