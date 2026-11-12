using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BCIT_Social.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Communities",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Communities");
        }
    }
}
