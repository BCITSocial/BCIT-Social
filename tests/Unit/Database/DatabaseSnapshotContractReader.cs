using BCIT_Social.Tests.Support;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BCIT_Social.Tests.Unit.Database;

/// <summary>
/// Reads entity mappings from EF Core's generated model snapshot.
/// </summary>
internal static class DatabaseSnapshotContractReader
{
    /// <summary>
    /// Represents one modelBuilder.Entity mapping block.
    /// </summary>
    /// <param name="FullName">The entity's complete namespace and name.</param>
    /// <param name="Name">The entity's short name.</param>
    /// <param name="Statements">The compact statements in the mapping.</param>
    internal sealed record EntityMapping(
        string FullName,
        string Name,
        string[] Statements);

    /// <summary>
    /// Loads every entity-mapping block from the model snapshot.
    /// </summary>
    internal static IReadOnlyList<EntityMapping> LoadMappings()
    {
        var path = TestHelpers.WebProjectPath(
            "Data",
            "Migrations",
            "ApplicationDbContextModelSnapshot.cs");

        var root = CSharpSyntaxTree
            .ParseText(File.ReadAllText(path), path: path)
            .GetCompilationUnitRoot();

        var mappings = new List<EntityMapping>();

        foreach (var invocation in root
                     .DescendantNodes()
                     .OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression
                    is not MemberAccessExpressionSyntax access ||
                access.Name.Identifier.ValueText != "Entity")
            {
                continue;
            }

            var arguments = invocation.ArgumentList.Arguments;

            if (arguments.Count < 2 ||
                arguments[0].Expression
                    is not LiteralExpressionSyntax entityLiteral ||
                arguments[1].Expression
                    is not SimpleLambdaExpressionSyntax entityLambda)
            {
                continue;
            }

            var fullName = entityLiteral.Token.ValueText;
            var name = fullName.Split('.').Last();

            var statements = entityLambda
                .DescendantNodes()
                .OfType<ExpressionStatementSyntax>()
                .Select(statement => Compact(statement.ToFullString()))
                .ToArray();

            mappings.Add(new EntityMapping(
                fullName,
                name,
                statements));
        }

        return mappings;
    }

    /// <summary>
    /// Returns the table and column mapping for an application entity.
    /// </summary>
    internal static EntityMapping SchemaMapping(string entityName)
    {
        return LoadMappings().Single(mapping =>
            mapping.FullName ==
                $"BCIT_Social.Models.{entityName}" &&
            mapping.Statements.Any(statement =>
                statement.Contains(
                    ".ToTable(",
                    StringComparison.Ordinal)));
    }

    /// <summary>
    /// Returns the foreign-key relationship mapping for an application entity.
    /// </summary>
    internal static EntityMapping RelationshipMapping(
        string entityName)
    {
        return LoadMappings().Single(mapping =>
            mapping.FullName ==
                $"BCIT_Social.Models.{entityName}" &&
            mapping.Statements.Any(statement =>
                statement.StartsWith(
                    "b.HasOne(",
                    StringComparison.Ordinal)));
    }

    /// <summary>
    /// Finds one foreign-key relationship statement.
    /// </summary>
    internal static string FindRelationship(
        string dependentEntity,
        string principalEntity,
        string foreignKey)
    {
        var mapping = RelationshipMapping(dependentEntity);

        var principalMarker =
            $"b.HasOne(\"BCIT_Social.Models.{principalEntity}\"";

        var foreignKeyMarker =
            $"\"{foreignKey}\")";

        return mapping.Statements.Single(statement =>
            statement.StartsWith(
                principalMarker,
                StringComparison.Ordinal) &&
            statement.Contains(
                foreignKeyMarker,
                StringComparison.Ordinal));
    }

    /// <summary>
    /// Removes whitespace so checks do not depend on source formatting.
    /// </summary>
    private static string Compact(string value)
    {
        return string.Concat(
            value.Where(character => !char.IsWhiteSpace(character)));
    }
}