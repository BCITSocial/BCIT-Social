namespace BCIT_Social.Tests.Unit.Database;

/// <summary>
/// Verifies the unique indexes recorded in EF Core's model snapshot.
///
/// These tests inspect configuration and do not connect to SQL Server.
/// </summary>
public class UniqueConstraintTests
{
    /// <summary>
    /// Verifies that application properties which must not contain duplicate
    /// values have unique indexes.
    /// </summary>
    /// <param name="entity">The entity containing the index.</param>
    /// <param name="property">The uniquely indexed property.</param>
    [Theory]
    [InlineData("PostPhoto", "StorageKey")]
    [InlineData("FriendRelationship", "RelationshipKey")]
    public void Snapshot_DeclaresExpectedUniqueIndex(
        string entity,
        string property)
    {
        var mapping =
            DatabaseSnapshotContractReader.SchemaMapping(entity);

        var indexStatement = mapping.Statements.SingleOrDefault(
            statement =>
                statement.StartsWith(
                    $"b.HasIndex(\"{property}\")",
                    StringComparison.Ordinal));

        indexStatement.ShouldNotBeNull(
            $"{entity}.{property} should have an index.");

        indexStatement!.ShouldContain(".IsUnique()");

    }   
    /// <summary>
    /// Verifies that each custom unique string column has a bounded SQL length.
    /// SQL Server cannot use an unbounded nvarchar(max) column as a conventional
    /// index key.
    /// </summary>
    /// <param name="entity">The entity containing the unique property.</param>
    /// <param name="property">The unique string property.</param>
    [Theory]
    [InlineData("PostPhoto", "StorageKey")]
    [InlineData("FriendRelationship", "RelationshipKey")]
    public void UniqueStringColumn_UsesBoundedSqlType(
        string entity,
        string property)
    {
        var mapping =
            DatabaseSnapshotContractReader.SchemaMapping(entity);

        var propertyStatement = mapping.Statements.SingleOrDefault(
            statement =>
                statement.StartsWith(
                    $"b.Property<string>(\"{property}\")",
                    StringComparison.Ordinal));

        propertyStatement.ShouldNotBeNull(
            $"{entity}.{property} should be mapped.");

        propertyStatement!.ShouldContain(
            ".HasColumnType(\"nvarchar(450)\")");
    }

    /// <summary>
    /// Verifies that the Identity normalized username also remains uniquely
    /// indexed, preventing duplicate normalized usernames.
    /// </summary>
    [Fact]
    public void ApplicationUser_NormalizedUserName_IsUnique()
    {
        var mapping =
            DatabaseSnapshotContractReader.SchemaMapping(
                "ApplicationUser");

        var indexStatement = mapping.Statements.SingleOrDefault(
            statement =>
                statement.StartsWith(
                    "b.HasIndex(\"NormalizedUserName\")",
                    StringComparison.Ordinal));

        indexStatement.ShouldNotBeNull();
        indexStatement!.ShouldContain(".IsUnique()");
        indexStatement.ShouldContain(
            ".HasFilter(\"[NormalizedUserName]ISNOTNULL\")");
    }
}