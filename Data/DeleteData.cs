using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BCIT_Social.Data;

/// <summary>
/// Permanently removes all application data while preserving the database
/// schema and EF Core migration history.
/// </summary>
public static class DeleteData
{
    private const string CONFIRMATION_PHRASE =
        "DELETE ALL BCIT SOCIAL DATA";

    private const string DELETE_SQL = """
        DECLARE @disableConstraintsSql nvarchar(max) = N'';
        DECLARE @deleteDataSql nvarchar(max) = N'';
        DECLARE @resetIdentitySql nvarchar(max) = N'';
        DECLARE @enableConstraintsSql nvarchar(max) = N'';

        -- Disable foreign-key and check constraints on every application table.
        SELECT @disableConstraintsSql +=
            N'ALTER TABLE '
            + QUOTENAME(schemaInfo.name)
            + N'.'
            + QUOTENAME(tableInfo.name)
            + N' NOCHECK CONSTRAINT ALL;'
            + CHAR(13)
            + CHAR(10)
        FROM sys.tables AS tableInfo
        INNER JOIN sys.schemas AS schemaInfo
            ON schemaInfo.schema_id = tableInfo.schema_id
        WHERE tableInfo.is_ms_shipped = 0
          AND tableInfo.name <> N'__EFMigrationsHistory';

        EXEC sys.sp_executesql @disableConstraintsSql;

        -- Delete every row while leaving the tables themselves intact.
        SELECT @deleteDataSql +=
            N'DELETE FROM '
            + QUOTENAME(schemaInfo.name)
            + N'.'
            + QUOTENAME(tableInfo.name)
            + N';'
            + CHAR(13)
            + CHAR(10)
        FROM sys.tables AS tableInfo
        INNER JOIN sys.schemas AS schemaInfo
            ON schemaInfo.schema_id = tableInfo.schema_id
        WHERE tableInfo.is_ms_shipped = 0
          AND tableInfo.name <> N'__EFMigrationsHistory';

        EXEC sys.sp_executesql @deleteDataSql;

        -- Reset identity columns so newly seeded records begin at 1.
        SELECT @resetIdentitySql +=
            N'DBCC CHECKIDENT ('''
            + REPLACE(
                QUOTENAME(schemaInfo.name)
                + N'.'
                + QUOTENAME(tableInfo.name),
                N'''',
                N'''''')
            + N''', RESEED, 0) WITH NO_INFOMSGS;'
            + CHAR(13)
            + CHAR(10)
        FROM sys.tables AS tableInfo
        INNER JOIN sys.schemas AS schemaInfo
            ON schemaInfo.schema_id = tableInfo.schema_id
        WHERE tableInfo.is_ms_shipped = 0
          AND tableInfo.name <> N'__EFMigrationsHistory'
          AND EXISTS
          (
              SELECT 1
              FROM sys.identity_columns AS identityInfo
              WHERE identityInfo.object_id = tableInfo.object_id
          );

        EXEC sys.sp_executesql @resetIdentitySql;

        -- Re-enable every constraint and verify the now-empty tables.
        SELECT @enableConstraintsSql +=
            N'ALTER TABLE '
            + QUOTENAME(schemaInfo.name)
            + N'.'
            + QUOTENAME(tableInfo.name)
            + N' WITH CHECK CHECK CONSTRAINT ALL;'
            + CHAR(13)
            + CHAR(10)
        FROM sys.tables AS tableInfo
        INNER JOIN sys.schemas AS schemaInfo
            ON schemaInfo.schema_id = tableInfo.schema_id
        WHERE tableInfo.is_ms_shipped = 0
          AND tableInfo.name <> N'__EFMigrationsHistory';

        EXEC sys.sp_executesql @enableConstraintsSql;
        """;

    public static async Task DeleteAllAsync(
        ApplicationDbContext db,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(configuration);

        var resetEnabled =
            configuration.GetValue<bool>(
                "DatabaseReset:Enabled");

        var suppliedConfirmation =
            configuration["DatabaseReset:Confirmation"];

        if (!resetEnabled)
        {
            throw new InvalidOperationException(
                "Database deletion was requested, but " +
                "'DatabaseReset:Enabled' is not true.");
        }

        if (!string.Equals(
                suppliedConfirmation,
                CONFIRMATION_PHRASE,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Database deletion confirmation is missing or incorrect. " +
                $"The required confirmation is: {CONFIRMATION_PHRASE}");
        }

        var databaseName =
            db.Database.GetDbConnection().Database;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException(
                "The target database name could not be determined.");
        }

        Console.WriteLine(
            $"Deleting all application data from database '{databaseName}'.");

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            await db.Database.ExecuteSqlRawAsync(
                DELETE_SQL,
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            db.ChangeTracker.Clear();

            Console.WriteLine(
                $"All application data was deleted from database '{databaseName}'.");
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }
}