using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BCIT_Social.Tests.Support;

/// <summary>Small helpers shared by several test classes.</summary>
public static class TestHelpers
{
    /// <summary>Names of the properties that fail DataAnnotations validation, sorted for stable assertions.</summary>
    public static string[] InvalidMembers(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results.SelectMany(r => r.MemberNames).Distinct().OrderBy(n => n, StringComparer.Ordinal).ToArray();
    }

    /// <summary>"c=7&amp;filter=unread" – compares route-value dictionaries including key order.</summary>
    public static string Format(IDictionary<string, string> values) =>
        string.Join("&", values.Select(pair => $"{pair.Key}={pair.Value}"));

    /// <summary>Reads the project-relative path of the web project that was baked into the test assembly at build time.</summary>
    public static string WebProjectPath(params string[] relative)
    {
        var root = typeof(TestHelpers).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(a => a.Key == "WebProjectDir").Value!;
        return Path.GetFullPath(Path.Combine([root, .. relative]));
    }
}
