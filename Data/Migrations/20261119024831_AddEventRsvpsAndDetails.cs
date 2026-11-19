using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BCIT_Social.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventRsvpsAndDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Campus",
                table: "CommunityEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CommunityEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndsAt",
                table: "CommunityEvents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "CommunityEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "EventRsvps",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Going = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRsvps", x => new { x.EventId, x.UserId });
                    table.ForeignKey(
                        name: "FK_EventRsvps_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventRsvps_CommunityEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "CommunityEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventRsvps_UserId",
                table: "EventRsvps",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRsvps");

            migrationBuilder.DropColumn(
                name: "Campus",
                table: "CommunityEvents");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CommunityEvents");

            migrationBuilder.DropColumn(
                name: "EndsAt",
                table: "CommunityEvents");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "CommunityEvents");
        }
    }
}
