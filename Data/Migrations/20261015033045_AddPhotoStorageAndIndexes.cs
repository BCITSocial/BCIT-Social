using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BCIT_Social.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoStorageAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Posts_AuthorId",
                table: "Posts");

            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoContentType",
                table: "Profiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProfilePhotoFileSizeBytes",
                table: "Profiles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoOriginalFileName",
                table: "Profiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoStorageKey",
                table: "Profiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProfilePhotoUpdatedAt",
                table: "Profiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BannerPhotoContentType",
                table: "Communities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "BannerPhotoFileSizeBytes",
                table: "Communities",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BannerPhotoOriginalFileName",
                table: "Communities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BannerPhotoStorageKey",
                table: "Communities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BannerPhotoUpdatedAt",
                table: "Communities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PostPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostPhotos_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_AuthorId_CreatedAt",
                table: "Posts",
                columns: new[] { "AuthorId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PostPhotos_PostId_DisplayOrder",
                table: "PostPhotos",
                columns: new[] { "PostId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PostPhotos_StorageKey",
                table: "PostPhotos",
                column: "StorageKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostPhotos");

            migrationBuilder.DropIndex(
                name: "IX_Posts_AuthorId_CreatedAt",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoContentType",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoFileSizeBytes",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoOriginalFileName",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoStorageKey",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoUpdatedAt",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "BannerPhotoContentType",
                table: "Communities");

            migrationBuilder.DropColumn(
                name: "BannerPhotoFileSizeBytes",
                table: "Communities");

            migrationBuilder.DropColumn(
                name: "BannerPhotoOriginalFileName",
                table: "Communities");

            migrationBuilder.DropColumn(
                name: "BannerPhotoStorageKey",
                table: "Communities");

            migrationBuilder.DropColumn(
                name: "BannerPhotoUpdatedAt",
                table: "Communities");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_AuthorId",
                table: "Posts",
                column: "AuthorId");
        }
    }
}
