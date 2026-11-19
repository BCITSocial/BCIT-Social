using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BCIT_Social.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeededEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Match the original seeded title, creator and community so unrelated events are untouched.
            migrationBuilder.Sql("""
                UPDATE e
                SET e.DateOfDel = SYSUTCDATETIME()
                FROM CommunityEvents AS e
                INNER JOIN AspNetUsers AS u ON u.Id = e.CreatorId
                INNER JOIN Communities AS c ON c.Id = e.CommunityId
                INNER JOIN (VALUES
                    ('BCIT Sample Event 01', 'maya.chen@example.com', 'BCIT Developers'),
                    ('BCIT Sample Event 02', 'noah.patel@example.com', 'Burnaby Study Circle'),
                    ('BCIT Sample Event 03', 'ava.williams@example.com', 'BCIT Student Community 03'),
                    ('BCIT Sample Event 04', 'liam.garcia@example.com', 'BCIT Student Community 04'),
                    ('BCIT Sample Event 05', 'emma.johnson@example.com', 'BCIT Student Community 05'),
                    ('BCIT Sample Event 06', 'oliver.kim@example.com', 'BCIT Student Community 06'),
                    ('BCIT Sample Event 07', 'sophia.brown@example.com', 'BCIT Student Community 07'),
                    ('BCIT Sample Event 08', 'ethan.lee@example.com', 'BCIT Student Community 08'),
                    ('BCIT Sample Event 09', 'isabella.martin@example.com', 'BCIT Student Community 09'),
                    ('BCIT Sample Event 10', 'lucas.thompson@example.com', 'BCIT Student Community 10'),
                    ('BCIT Sample Event 11', 'mia.anderson@example.com', 'BCIT Student Community 11'),
                    ('BCIT Sample Event 12', 'mason.taylor@example.com', 'BCIT Student Community 12'),
                    ('BCIT Sample Event 13', 'amelia.moore@example.com', 'BCIT Student Community 13'),
                    ('BCIT Sample Event 14', 'james.jackson@example.com', 'BCIT Student Community 14'),
                    ('BCIT Sample Event 15', 'harper.white@example.com', 'BCIT Student Community 15'),
                    ('BCIT Sample Event 16', 'benjamin.harris@example.com', 'BCIT Student Community 16'),
                    ('BCIT Sample Event 17', 'evelyn.clark@example.com', 'BCIT Student Community 17'),
                    ('BCIT Sample Event 18', 'henry.lewis@example.com', 'BCIT Student Community 18'),
                    ('BCIT Sample Event 19', 'charlotte.walker@example.com', 'BCIT Student Community 19'),
                    ('BCIT Sample Event 20', 'daniel.hall@example.com', 'BCIT Student Community 20')
                ) AS seed(Title, Email, CommunityName)
                    ON e.Title = seed.Title AND u.Email = seed.Email AND c.Name = seed.CommunityName
                WHERE e.DateOfDel IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Do not resurrect deleted sample events when rolling back.
        }
    }
}
