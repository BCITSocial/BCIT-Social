using BCIT_Social.Tests.Support;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BCIT_Social.Tests.Unit.Database;

/// <summary>
/// Verifies the database schema recorded in EF Core's current model snapshot.
///
/// These tests inspect the generated snapshot source and do not connect to a
/// local or remote database.
/// </summary>
public class DatabaseSchemaTests
{
    /// <summary>
    /// Represents one entity and table mapping extracted from the EF model
    /// snapshot.
    /// </summary>
    /// <param name="Name">The entity's short class name.</param>
    /// <param name="Table">The SQL table name.</param>
    /// <param name="Statements">
    /// The compact mapping statements declared for the entity.
    /// </param>
    private sealed record SnapshotEntity(
        string Name,
        string Table,
        string[] Statements);

    /// <summary>
    /// Loads and parses the current EF Core model snapshot.
    /// </summary>
    private static CompilationUnitSyntax LoadSnapshot()
    {
        var path = TestHelpers.WebProjectPath(
            "Data",
            "Migrations",
            "ApplicationDbContextModelSnapshot.cs");

        return CSharpSyntaxTree
            .ParseText(File.ReadAllText(path), path: path)
            .GetCompilationUnitRoot();
    }

    /// <summary>
    /// Removes whitespace so assertions do not depend on source formatting.
    /// </summary>
    private static string Compact(string value)
    {
        return string.Concat(
            value.Where(character => !char.IsWhiteSpace(character)));
    }

    /// <summary>
    /// Determines whether an invocation is an EF modelBuilder.Entity call.
    /// </summary>
    private static bool IsEntityInvocation(
        InvocationExpressionSyntax invocation)
    {
        return invocation.Expression is MemberAccessExpressionSyntax access &&
               access.Name.Identifier.ValueText == "Entity";
    }

    /// <summary>
    /// Determines whether an invocation is an EF ToTable call.
    /// </summary>
    private static bool IsToTableInvocation(
        InvocationExpressionSyntax invocation)
    {
        return invocation.Expression is MemberAccessExpressionSyntax access &&
               access.Name.Identifier.ValueText == "ToTable";
    }

    /// <summary>
    /// Extracts the entity, table, and column mapping blocks from the current
    /// EF Core model snapshot.
    /// </summary>
    private static IReadOnlyDictionary<string, SnapshotEntity>
        LoadSchemaEntities()
    {
        var entities = new Dictionary<string, SnapshotEntity>(
            StringComparer.Ordinal);

        var invocations = LoadSnapshot()
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(IsEntityInvocation);

        foreach (var invocation in invocations)
        {
            var arguments = invocation.ArgumentList.Arguments;

            if (arguments.Count < 2 ||
                arguments[0].Expression is not LiteralExpressionSyntax
                    entityLiteral ||
                arguments[1].Expression is not SimpleLambdaExpressionSyntax
                    entityLambda)
            {
                continue;
            }

            var tableInvocation = entityLambda
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault(IsToTableInvocation);

            if (tableInvocation is null ||
                tableInvocation.ArgumentList.Arguments.Count == 0 ||
                tableInvocation.ArgumentList.Arguments[0].Expression
                    is not LiteralExpressionSyntax tableLiteral)
            {
                continue;
            }

            var fullEntityName =
                entityLiteral.Token.ValueText;

            var entityName = fullEntityName
                .Split('.')
                .Last();

            var tableName =
                tableLiteral.Token.ValueText;

            var statements = entityLambda
                .DescendantNodes()
                .OfType<ExpressionStatementSyntax>()
                .Select(statement => Compact(statement.ToFullString()))
                .ToArray();

            entities[entityName] = new SnapshotEntity(
                entityName,
                tableName,
                statements);
        }

        return entities;
    }

    /// <summary>
    /// Returns the snapshot statement that maps a particular entity property.
    /// </summary>
    /// <param name="entityName">The entity containing the property.</param>
    /// <param name="propertyName">The mapped property name.</param>
    private static string FindPropertyMapping(
        string entityName,
        string propertyName)
    {
        var entities = LoadSchemaEntities();

        entities.ShouldContainKey(
            entityName,
            $"The snapshot should contain the {entityName} entity.");

        var marker = $">(\"{propertyName}\")";

        var statement = entities[entityName]
            .Statements
            .SingleOrDefault(candidate =>
                candidate.StartsWith(
                    "b.Property<",
                    StringComparison.Ordinal) &&
                candidate.Contains(
                    marker,
                    StringComparison.Ordinal));

        statement.ShouldNotBeNull(
            $"{entityName}.{propertyName} should be mapped in the snapshot.");

        return statement!;
    }

    /// <summary>
    /// Verifies that the model snapshot contains every expected application
    /// and ASP.NET Core Identity table.
    /// </summary>
    [Fact]
    public void Snapshot_ContainsEveryExpectedTable()
    {
        var actual = LoadSchemaEntities()
            .Values
            .Select(entity => entity.Table)
            .OrderBy(table => table, StringComparer.Ordinal)
            .ToArray();

        var expected = new[]
        {
            "AspNetRoleClaims",
            "AspNetRoles",
            "AspNetUserClaims",
            "AspNetUserLogins",
            "AspNetUserRoles",
            "AspNetUsers",
            "AspNetUserTokens",
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
        .OrderBy(table => table, StringComparer.Ordinal)
        .ToArray();

        actual.ShouldBe(expected);
    }

    /// <summary>
    /// Verifies that important properties use the expected CLR and SQL Server
    /// types in the current schema snapshot.
    /// </summary>
    /// <param name="entity">The entity containing the property.</param>
    /// <param name="property">The property being inspected.</param>
    /// <param name="clrType">The CLR type recorded in the snapshot.</param>
    /// <param name="sqlType">The expected SQL Server column type.</param>
    [Theory]
    [InlineData(
        "ApplicationUser",
        "Id",
        "string",
        "nvarchar(450)")]
    [InlineData(
        "ApplicationUser",
        "CreatedAt",
        "DateTime",
        "datetime2")]
    [InlineData(
        "ApplicationUser",
        "FirstName",
        "string",
        "nvarchar(max)")]
    [InlineData(
        "ApplicationUser",
        "LastName",
        "string",
        "nvarchar(max)")]
    [InlineData(
        "Bookmarks",
        "UserId",
        "string",
        "nvarchar(450)")]
    [InlineData(
        "Bookmarks",
        "PostId",
        "int",
        "int")]
    [InlineData(
        "Bookmarks",
        "SavedAt",
        "DateTime",
        "datetime2")]
    [InlineData(
        "Post",
        "Id",
        "int",
        "int")]
    [InlineData(
        "Post",
        "AuthorId",
        "string",
        "nvarchar(450)")]
    [InlineData(
        "Post",
        "CommunityId",
        "int?",
        "int")]
    [InlineData(
        "Post",
        "CreatedAt",
        "DateTime",
        "datetime2")]
    [InlineData(
        "PostPhoto",
        "FileSizeBytes",
        "long",
        "bigint")]
    [InlineData(
        "PostPhoto",
        "StorageKey",
        "string",
        "nvarchar(450)")]
    [InlineData(
        "CommunityMember",
        "IsModerator",
        "bool",
        "bit")]
    [InlineData(
        "CommunityEvent",
        "EventDate",
        "DateTime",
        "datetime2")]
    [InlineData(
        "MessageReadReceipt",
        "ReadAt",
        "DateTime",
        "datetime2")]
    public void ImportantColumns_HaveExpectedTypes(
        string entity,
        string property,
        string clrType,
        string sqlType)
    {
        var mapping = FindPropertyMapping(entity, property);

        mapping.ShouldContain(
            $"b.Property<{clrType}>(\"{property}\")");

        mapping.ShouldContain(
            $".HasColumnType(\"{sqlType}\")");
    }

    /// <summary>
    /// Verifies that important required text columns are marked as required in
    /// the EF model snapshot.
    /// </summary>
    /// <param name="entity">The entity containing the required column.</param>
    /// <param name="property">The required property being inspected.</param>
    [Theory]
    [InlineData("ApplicationUser", "FirstName")]
    [InlineData("ApplicationUser", "LastName")]
    [InlineData("Community", "CreatorId")]
    [InlineData("Community", "Name")]
    [InlineData("Community", "Type")]
    [InlineData("CommunityEvent", "Campus")]
    [InlineData("CommunityEvent", "Description")]
    [InlineData("CommunityEvent", "Location")]
    [InlineData("CommunityEvent", "Title")]
    [InlineData("CommunityMessage", "MessageBody")]
    [InlineData("FriendRelationship", "RelationshipKey")]
    [InlineData("FriendRelationship", "RequesterId")]
    [InlineData("FriendRelationship", "RecipientId")]
    [InlineData("Message", "Text")]
    [InlineData("Post", "AuthorId")]
    [InlineData("Post", "Content")]
    [InlineData("Post", "Title")]
    [InlineData("PostPhoto", "StorageKey")]
    [InlineData("Profile", "Bio")]
    [InlineData("Profile", "Campus")]
    [InlineData("Profile", "DisplayName")]
    [InlineData("Profile", "Program")]
    [InlineData("Tags", "Name")]
    public void RequiredColumns_AreMarkedRequired(
        string entity,
        string property)
    {
        var mapping = FindPropertyMapping(entity, property);

        mapping.ShouldContain(".IsRequired()");
    }

    /// <summary>
    /// Verifies that important optional columns are not marked as required.
    /// This protects soft-deletion fields and optional relationships.
    /// </summary>
    /// <param name="entity">The entity containing the optional column.</param>
    /// <param name="property">The optional property being inspected.</param>
    [Theory]
    [InlineData("ApplicationUser", "DateOfDel")]
    [InlineData("Chatbox", "CreatorId")]
    [InlineData("Comments", "AuthorId")]
    [InlineData("Comments", "ParentCommentId")]
    [InlineData("Community", "DateOfDel")]
    [InlineData("Community", "Description")]
    [InlineData("CommunityEvent", "CommunityId")]
    [InlineData("CommunityEvent", "CreatorId")]
    [InlineData("CommunityEvent", "EndsAt")]
    [InlineData("Message", "SenderId")]
    [InlineData("Post", "CommunityId")]
    [InlineData("Post", "DateOfDel")]
    [InlineData("Profile", "ProfilePhotoStorageKey")]
    public void OptionalColumns_AreNotMarkedRequired(
        string entity,
        string property)
    {
        var mapping = FindPropertyMapping(entity, property);

        mapping.ShouldNotContain(".IsRequired()");
    }

    /// <summary>
    /// Verifies that entities with generated integer identifiers use SQL
    /// Server identity columns.
    /// </summary>
    /// <param name="entity">
    /// The entity expected to have an identity-generated Id column.
    /// </param>
    [Theory]
    [InlineData("Chatbox")]
    [InlineData("ChatboxMembershipHistory")]
    [InlineData("Comments")]
    [InlineData("Community")]
    [InlineData("CommunityEvent")]
    [InlineData("CommunityMessage")]
    [InlineData("FriendRelationship")]
    [InlineData("Message")]
    [InlineData("Post")]
    [InlineData("PostPhoto")]
    [InlineData("Tags")]
    public void IntegerIdentifiers_UseSqlServerIdentityColumns(
        string entity)
    {
        var entities = LoadSchemaEntities();

        entities.ShouldContainKey(
            entity,
            $"The snapshot should contain the {entity} entity.");

        var hasIdentityColumn = entities[entity]
            .Statements
            .Any(statement =>
                statement.Contains(
                    "UseIdentityColumn(b.Property<int>(\"Id\"))",
                    StringComparison.Ordinal));

        hasIdentityColumn.ShouldBeTrue(
            $"{entity}.Id should use a SQL Server identity column.");
    }
}