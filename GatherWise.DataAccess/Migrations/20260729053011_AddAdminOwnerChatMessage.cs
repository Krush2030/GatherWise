using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GatherWise.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminOwnerChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminOwnerChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserReportId = table.Column<int>(type: "int", nullable: false),
                    SenderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminOwnerChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminOwnerChatMessages_UserReports_UserReportId",
                        column: x => x.UserReportId,
                        principalTable: "UserReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminOwnerChatMessages_UserReportId",
                table: "AdminOwnerChatMessages",
                column: "UserReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminOwnerChatMessages");
        }
    }
}
