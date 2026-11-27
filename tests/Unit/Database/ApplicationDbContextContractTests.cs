using BCIT_Social.Tests.Support;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BCIT_Social.Tests.Unit.Database;

/// <summary>
/// Verifies the important Entity Framework configuration declared in
/// ApplicationDbContext.cs without connecting to a database.
/// </summary>
public class ApplicationDbContextContractTests
{
    /// <summary>
    /// Loads ApplicationDbContext.cs and parses it into a Roslyn syntax tree.
    /// </summary>
    private static CompilationUnitSyntax LoadContext()
    {
        var path = TestHelpers.WebProjectPath(
            "Data",
            "ApplicationDbContext.cs");

        return CSharpSyntaxTree
            .ParseText(File.ReadAllText(path), path: path)
            .GetCompilationUnitRoot();
    }

    /// <summary>
    /// Removes whitespace so mapping checks do not depend on source formatting.
    /// </summary>
    private static string Compact(string value)
    {
        return string.Concat(
            value.Where(character => !char.IsWhiteSpace(character)));
    }

    /// <summary>
    /// Returns the mapping statements from ApplicationDbContext in a compact
    /// format that is easier to inspect.
    /// </summary>
    private static string[] MappingStatements()
    {
        return LoadContext()
            .DescendantNodes()
            .OfType<ExpressionStatementSyntax>()
            .Select(statement => Compact(statement.ToFullString()))
            .ToArray();
    }

    /// <summary>
    /// Verifies that ApplicationDbContext declares every DbSet required by the
    /// application. This detects accidental removal or renaming of a DbSet.
    /// </summary>
    [Fact]
    public void Context_DeclaresEveryExpectedDbSet()
    {
        var actual = LoadContext()
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Where(property =>
                property.Type is GenericNameSyntax generic &&
                generic.Identifier.ValueText == "DbSet")
            .Select(property => property.Identifier.ValueText)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        var expected = new[]
        {
            "Bookmarks",
            "ChatboxMembershipHistory",
            "Chatboxes",
            "ChatboxUsers",
            "Comments",
            "Communities",
            "CommunityEvents",
            "CommunityMembers",
            "CommunityMessages",
            "EventRsvps",
            "FriendRelationships",
            "MessageReadReceipts",
            "Messages",
            "PostPhotos",
            "Posts",
            "PostTags",
            "Profiles",
            "Tags"
        }
        .OrderBy(name => name, StringComparer.Ordinal)
        .ToArray();

        actual.ShouldBe(expected);
    }

    /// <summary>
    /// Verifies that junction entities use the expected two-column composite
    /// primary keys. These keys prevent duplicate relationships such as a user
    /// bookmarking the same post more than once.
    /// </summary>
    /// <param name="entity">The entity whose primary key is inspected.</param>
    /// <param name="firstProperty">The first primary-key property.</param>
    /// <param name="secondProperty">The second primary-key property.</param>
    [Theory]
    [InlineData("CommunityMember", "UserId", "CommunityId")]
    [InlineData("Bookmarks", "UserId", "PostId")]
    [InlineData("PostTags", "PostId", "TagId")]
    [InlineData("ChatboxUser", "UserId", "ChatboxId")]
    [InlineData("MessageReadReceipt", "MessageId", "UserId")]
    public void Context_ConfiguresExpectedCompositeKey(
        string entity,
        string firstProperty,
        string secondProperty)
    {
        var statement = MappingStatements().SingleOrDefault(candidate =>
            candidate.Contains(
                $"builder.Entity<{entity}>()",
                StringComparison.Ordinal) &&
            candidate.Contains(
                ".HasKey(",
                StringComparison.Ordinal));

        statement.ShouldNotBeNull(
            $"{entity} should have an explicitly configured composite key.");

        statement!.ShouldContain($".{firstProperty}");
        statement.ShouldContain($".{secondProperty}");
    }

    /// <summary>
    /// Verifies the expected behavior when a related row is deleted. Cascade
    /// removes dependent rows, Restrict prevents deletion, and SetNull keeps
    /// the dependent row while clearing its foreign key.
    /// </summary>
    /// <param name="entity">The dependent entity being inspected.</param>
    /// <param name="foreignKey">The configured foreign-key property.</param>
    /// <param name="deleteBehavior">The expected EF Core delete behavior.</param>
    [Theory]
    [InlineData("Profile", "UserId", "Cascade")]
    [InlineData("Community", "CreatorId", "Restrict")]
    [InlineData("CommunityMember", "UserId", "Restrict")]
    [InlineData("CommunityMember", "CommunityId", "Cascade")]
    [InlineData("CommunityEvent", "CreatorId", "SetNull")]
    [InlineData("CommunityEvent", "CommunityId", "SetNull")]
    [InlineData("CommunityMessage", "CommunityId", "Cascade")]
    [InlineData("CommunityMessage", "SenderId", "SetNull")]
    [InlineData("Post", "AuthorId", "Restrict")]
    [InlineData("Post", "CommunityId", "SetNull")]
    [InlineData("PostPhoto", "PostId", "Cascade")]
    [InlineData("Comments", "AuthorId", "SetNull")]
    [InlineData("Comments", "PostId", "Cascade")]
    [InlineData("Comments", "ParentCommentId", "Restrict")]
    [InlineData("Bookmarks", "UserId", "Restrict")]
    [InlineData("Bookmarks", "PostId", "Cascade")]
    [InlineData("EventRsvp", "EventId", "Cascade")]
    [InlineData("EventRsvp", "UserId", "Restrict")]
    [InlineData("PostTags", "PostId", "Cascade")]
    [InlineData("PostTags", "TagId", "Cascade")]
    [InlineData("Chatbox", "CreatorId", "SetNull")]
    [InlineData("ChatboxUser", "UserId", "Restrict")]
    [InlineData("ChatboxUser", "ChatboxId", "Cascade")]
    [InlineData("Message", "ChatboxId", "Cascade")]
    [InlineData("Message", "SenderId", "SetNull")]
    [InlineData("MessageReadReceipt", "MessageId", "Cascade")]
    [InlineData("MessageReadReceipt", "UserId", "Cascade")]
    [InlineData("ChatboxMembershipHistory", "UserId", "SetNull")]
    [InlineData("ChatboxMembershipHistory", "ChatboxId", "Cascade")]
    [InlineData("FriendRelationship", "RequesterId", "Restrict")]
    [InlineData("FriendRelationship", "RecipientId", "Restrict")]
    public void Context_ConfiguresExpectedDeleteBehavior(
        string entity,
        string foreignKey,
        string deleteBehavior)
    {
        var statement = MappingStatements().SingleOrDefault(candidate =>
            candidate.Contains(
                $"builder.Entity<{entity}>()",
                StringComparison.Ordinal) &&
            (candidate.Contains(
                 ".HasForeignKey(",
                 StringComparison.Ordinal) ||
             candidate.Contains(
                 $".HasForeignKey<{entity}>(",
                 StringComparison.Ordinal)) &&
            candidate.Contains(
                $".{foreignKey})",
                StringComparison.Ordinal));

        statement.ShouldNotBeNull(
            $"{entity}.{foreignKey} should have an explicit relationship.");

        statement!.ShouldContain(
            $".OnDelete(DeleteBehavior.{deleteBehavior})");
    }

    /// <summary>
    /// Verifies that properties which must not contain duplicate values have
    /// unique indexes. This protects photo storage keys and relationship keys.
    /// </summary>
    /// <param name="entity">The entity containing the unique index.</param>
    /// <param name="property">The property that must contain unique values.</param>
    [Theory]
    [InlineData("PostPhoto", "StorageKey")]
    [InlineData("FriendRelationship", "RelationshipKey")]
    public void Context_ConfiguresExpectedUniqueIndex(
        string entity,
        string property)
    {
        var statement = MappingStatements().SingleOrDefault(candidate =>
            candidate.Contains(
                $"builder.Entity<{entity}>()",
                StringComparison.Ordinal) &&
            candidate.Contains(
                ".HasIndex(",
                StringComparison.Ordinal) &&
            candidate.Contains(
                $".{property})",
                StringComparison.Ordinal));

        statement.ShouldNotBeNull(
            $"{entity}.{property} should have an index.");

        statement!.ShouldContain(".IsUnique()");
    }

    /// <summary>
    /// Verifies that Profile uses UserId as its primary key. This supports the
    /// one-to-one relationship and prevents a user from having multiple profiles.
    /// </summary>
    [Fact]
    public void Profile_UsesUserIdAsItsPrimaryKey()
    {
        var statement = MappingStatements().SingleOrDefault(candidate =>
            candidate.Contains(
                "builder.Entity<Profile>()",
                StringComparison.Ordinal) &&
            candidate.Contains(
                ".HasKey(",
                StringComparison.Ordinal));

        statement.ShouldNotBeNull(
            "Profile should have an explicitly configured primary key.");

        statement!.ShouldContain(".UserId");
    }
}