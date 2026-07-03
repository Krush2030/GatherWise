using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GatherWise.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorServicesAndMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "ServiceCategory",
                table: "Vendors");

            migrationBuilder.CreateTable(
                name: "VendorServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ServiceCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ServicePhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PerNumberOfPersons = table.Column<int>(type: "int", nullable: false),
                    MainPhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorServices_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendorServiceImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorServiceId = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorServiceImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorServiceImages_VendorServices_VendorServiceId",
                        column: x => x.VendorServiceId,
                        principalTable: "VendorServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VendorServiceImages_VendorServiceId",
                table: "VendorServiceImages",
                column: "VendorServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorServices_VendorId",
                table: "VendorServices",
                column: "VendorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VendorServiceImages");

            migrationBuilder.DropTable(
                name: "VendorServices");

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "Vendors",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ServiceCategory",
                table: "Vendors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
