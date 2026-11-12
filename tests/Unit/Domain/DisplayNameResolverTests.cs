using BCIT_Social.Core.Profiles;

namespace BCIT_Social.Tests.Unit.Domain;

public class DisplayNameResolverTests
{
    [Theory]
    [InlineData("BCIT Social User", new[] { "Maya", "maya@my.bcit.ca" }, "Maya")]           // profile name wins
    [InlineData("BCIT Social User", new[] { null, "maya@my.bcit.ca" }, "maya@my.bcit.ca")]  // no profile → user name
    [InlineData("BCIT Social User", new[] { "", "maya@my.bcit.ca" }, "maya@my.bcit.ca")]
    [InlineData("BCIT Social User", new[] { "   ", "maya@my.bcit.ca" }, "maya@my.bcit.ca")] // blank is not a name
    [InlineData("BCIT Social User", new[] { "  Maya  " }, "Maya")]
    [InlineData("Alumni member", new string?[] { null, null }, "Alumni member")]           
    [InlineData("", new string?[0], "")]
    public void Resolve_ReturnsFirstNonBlankCandidate_ElseFallback(string fallback, string?[] candidates, string expected)
    {
        DisplayNameResolver.Resolve(fallback, candidates).ShouldBe(expected);
    }
}
