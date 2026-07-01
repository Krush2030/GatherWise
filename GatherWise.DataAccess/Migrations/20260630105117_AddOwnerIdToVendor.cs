using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GatherWise.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerIdToVendor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Vendors",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_OwnerId",
                table: "Vendors",
                column: "OwnerId");

            // FIXED: Changed Cascade to NoAction to prevent multiple cascade paths
            migrationBuilder.AddForeignKey(
                name: "FK_Vendors_AspNetUsers_OwnerId",
                table: "Vendors",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vendors_AspNetUsers_OwnerId",
                table: "Vendors");

            migrationBuilder.DropIndex(
                name: "IX_Vendors_OwnerId",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Vendors");
        }
    }
}