using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GatherWise.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Venues",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EventHostId",
                table: "Bookings",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Venues_OwnerId",
                table: "Venues",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_EventHostId",
                table: "Bookings",
                column: "EventHostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_EventHostId",
                table: "Bookings",
                column: "EventHostId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Venues_AspNetUsers_OwnerId",
                table: "Venues",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_EventHostId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Venues_AspNetUsers_OwnerId",
                table: "Venues");

            migrationBuilder.DropIndex(
                name: "IX_Venues_OwnerId",
                table: "Venues");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_EventHostId",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Venues",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "EventHostId",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
