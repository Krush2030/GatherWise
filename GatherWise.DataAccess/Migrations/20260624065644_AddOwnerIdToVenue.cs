using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GatherWise.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerIdToVenue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Venues",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Venues");
        }
    }
}
