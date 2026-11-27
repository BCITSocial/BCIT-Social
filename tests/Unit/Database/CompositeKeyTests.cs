using BCIT_Social.Tests.Support;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BCIT_Social.Tests.Unit.Database;

/// <summary>
/// Verifies the composite primary keys recorded in EF Core's current model
/// snapshot.
///
/// These tests inspect schema configuration only and do not connect to a
/// database.
/// </summary>
public class CompositeKeyTests
{
    /// <summary>
    /// Represents an application entity extracted from the EF model snapshot.
    /// </summary>
    /// <param name="Name">The entity's short class name.</param>
    /// <param name="Statements">
    /// The compact schema statements declared for the entity.
    /// </param>
    private sealed record SnapshotEntity(
        string Name,
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
    /// Determines whether an entity mapping contains a table declaration.
    /// Relationship mappings later in the snapshot do not contain ToTable.
    /// </summary>
    private static bool ContainsTableMapping(
        SimpleLambdaExpressionSyntax entityLambda)
    {
        return entityLambda
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Any(invocation =>
                invocation.Expression
                    is MemberAccessExpressionSyntax access &&
                access.Name.Identifier.ValueText == "ToTable");
    }

    /// <summary>
    /// Extracts application-owned entity mappings from the EF model snapshot.
    /// ASP.NET Core Identity entities are excluded because their composite keys
    /// are owned by the Identity framework rather than this application.
    /// </summary>
    private static IReadOnlyDictionary<string, SnapshotEntity>
        LoadApplicationEntities()
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

            var fullEntityName = entityLiteral.Token.ValueText;

            if (!fullEntityName.StartsWith(
                    "BCIT_Social.Models.",
                    StringComparison.Ordinal) ||
                !ContainsTableMapping(entityLambda))
            {
                continue;
            }

            var entityName = fullEntityName
                .Split('.')
                .Last();

            var statements = entityLambda
                .DescendantNodes()
                .OfType<ExpressionStatementSyntax>()
                .Select(statement => Compact(statement.ToFullString()))
                .ToArray();

            entities[entityName] = new SnapshotEntity(
                entityName,
                statements);
        }

        return entities;
    }

    /// <summary>
    /// Returns one application entity from the model snapshot.
    /// </summary>
    /// <param name="entityName">The entity to retrieve.</param>
    private static SnapshotEntity FindEntity(string entityName)
    {
        var entities = LoadApplicationEntities();

        entities.ContainsKey(entityName).ShouldBeTrue(
            $"The model snapshot should contain {entityName}.");

        return entities[entityName];
    }

    /// <summary>
    /// Verifies that every application-owned junction entity has the expected
    /// two-column composite primary key in the correct order.
    ///
    /// Key order matters because SQL Server uses primary-key column order when
    /// creating and using the associated index.
    /// </summary>
    /// <param name="entity">The entity containing the composite key.</param>
    /// <param name="firstProperty">The first key column.</param>
    /// <param name="secondProperty">The second key column.</param>
    [Theory]
    [InlineData("Bookmarks", "UserId", "PostId")]
    [InlineData("ChatboxUser", "UserId", "ChatboxId")]
    [InlineData("CommunityMember", "UserId", "CommunityId")]
    [InlineData("EventRsvp", "EventId", "UserId")]
    [InlineData("MessageReadReceipt", "MessageId", "UserId")]
    [InlineData("PostTags", "PostId", "TagId")]
    public void ApplicationEntity_HasExpectedCompositePrimaryKey(
        string entity,
        string firstProperty,
        string secondProperty)
    {
        var snapshotEntity = FindEntity(entity);

        var expectedStatement =
            $"b.HasKey(\"{firstProperty}\",\"{secondProperty}\");";

        snapshotEntity.Statements.ShouldContain(
            expectedStatement,
            $"{entity} should use ({firstProperty}, {secondProperty}) " +
            "as its composite primary key.");
    }

    /// <summary>
    /// Verifies that the snapshot contains exactly the expected set of
    /// application-owned entities with composite primary keys.
    ///
    /// This detects an accidentally added or removed composite key.
    /// </summary>
    [Fact]
    public void Snapshot_ContainsOnlyExpectedApplicationCompositeKeys()
    {
        var actual = LoadApplicationEntities()
            .Values
            .Where(entity =>
                entity.Statements.Any(statement =>
                    statement.StartsWith(
                        "b.HasKey(",
                        StringComparison.Ordinal) &&
                    statement.Contains(
                        ',',
                        StringComparison.Ordinal)))
            .Select(entity => entity.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        var expected = new[]
        {
            "Bookmarks",
            "ChatboxUser",
            "CommunityMember",
            "EventRsvp",
            "MessageReadReceipt",
            "PostTags"
        }
        .OrderBy(name => name, StringComparer.Ordinal)
        .ToArray();

        actual.ShouldBe(expected);
    }

    /// <summary>
    /// Verifies that each composite-key column uses the expected SQL Server
    /// column type.
    /// </summary>
    /// <param name="entity">The entity containing the key column.</param>
    /// <param name="property">The key property being inspected.</param>
    /// <param name="clrType">The CLR type recorded in the snapshot.</param>
    /// <param name="sqlType">The expected SQL Server column type.</param>
    [Theory]
    [InlineData("Bookmarks", "UserId", "string", "nvarchar(450)")]
    [InlineData("Bookmarks", "PostId", "int", "int")]
    [InlineData("ChatboxUser", "UserId", "string", "nvarchar(450)")]
    [InlineData("ChatboxUser", "ChatboxId", "int", "int")]
    [InlineData("CommunityMember", "UserId", "string", "nvarchar(450)")]
    [InlineData("CommunityMember", "CommunityId", "int", "int")]
    [InlineData("EventRsvp", "EventId", "int", "int")]
    [InlineData("EventRsvp", "UserId", "string", "nvarchar(450)")]
    [InlineData("MessageReadReceipt", "MessageId", "int", "int")]
    [InlineData("MessageReadReceipt", "UserId", "string", "nvarchar(450)")]
    [InlineData("PostTags", "PostId", "int", "int")]
    [InlineData("PostTags", "TagId", "int", "int")]
    public void CompositeKeyColumn_HasExpectedSqlType(
        string entity,
        string property,
        string clrType,
        string sqlType)
    {
        var snapshotEntity = FindEntity(entity);

        var propertyStatement = snapshotEntity
            .Statements
            .SingleOrDefault(statement =>
                statement.StartsWith(
                    $"b.Property<{clrType}>(\"{property}\")",
                    StringComparison.Ordinal));

        propertyStatement.ShouldNotBeNull(
            $"{entity}.{property} should be mapped as {clrType}.");

        propertyStatement!.ShouldContain(
            $".HasColumnType(\"{sqlType}\")");
    }

    /// <summary>
    /// Verifies that composite-key entities do not use SQL Server identity
    /// columns. Their key values must come from their related records.
    /// </summary>
    /// <param name="entity">The composite-key entity being inspected.</param>
    [Theory]
    [InlineData("Bookmarks")]
    [InlineData("ChatboxUser")]
    [InlineData("CommunityMember")]
    [InlineData("EventRsvp")]
    [InlineData("MessageReadReceipt")]
    [InlineData("PostTags")]
    public void CompositeKeyEntity_DoesNotUseIdentityColumn(
        string entity)
    {
        var snapshotEntity = FindEntity(entity);

        var usesIdentityColumn = snapshotEntity
            .Statements
            .Any(statement =>
                statement.Contains(
                    "UseIdentityColumn",
                    StringComparison.Ordinal));

        usesIdentityColumn.ShouldBeFalse(
            $"{entity} should use its composite foreign-key values rather " +
            "than an identity-generated key.");
    }

    /// <summary>
    /// Verifies that the second relationship column in each composite key has
    /// a supporting index.
    ///
    /// The composite primary-key index begins with the first column, so a
    /// separate index helps queries and foreign-key checks that use the second
    /// column by itself.
    /// </summary>
    /// <param name="entity">The composite-key entity being inspected.</param>
    /// <param name="secondProperty">
    /// The second key column that requires a supporting index.
    /// </param>
    [Theory]
    [InlineData("Bookmarks", "PostId")]
    [InlineData("ChatboxUser", "ChatboxId")]
    [InlineData("CommunityMember", "CommunityId")]
    [InlineData("EventRsvp", "UserId")]
    [InlineData("MessageReadReceipt", "UserId")]
    [InlineData("PostTags", "TagId")]
    public void CompositeKey_HasIndexForSecondRelationshipColumn(
        string entity,
        string secondProperty)
    {
        var snapshotEntity = FindEntity(entity);

        snapshotEntity.Statements.ShouldContain(
            $"b.HasIndex(\"{secondProperty}\");",
            $"{entity}.{secondProperty} should have a supporting index.");
    }
}