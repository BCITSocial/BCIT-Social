using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BCIT_Social.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChatboxFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "ChatboxUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ChatType",
                table: "Chatboxes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Chatboxes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Chatboxes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChatboxMembershipHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChatboxId = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatboxMembershipHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatboxMembershipHistory_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatboxMembershipHistory_Chatboxes_ChatboxId",
                        column: x => x.ChatboxId,
                        principalTable: "Chatboxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageReadReceipts",
                columns: table => new
                {
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageReadReceipts", x => new { x.MessageId, x.UserId });
                    table.ForeignKey(
                        name: "FK_MessageReadReceipts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageReadReceipts_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatboxMembershipHistory_ChatboxId",
                table: "ChatboxMembershipHistory",
                column: "ChatboxId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatboxMembershipHistory_UserId",
                table: "ChatboxMembershipHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReadReceipts_UserId",
                table: "MessageReadReceipts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatboxMembershipHistory");

            migrationBuilder.DropTable(
                name: "MessageReadReceipts");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "ChatboxUsers");

            migrationBuilder.DropColumn(
                name: "ChatType",
                table: "Chatboxes");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Chatboxes");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Chatboxes");
        }
    }
}
