using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace coderush.Migrations
{
    /// <inheritdoc />
    public partial class AzureSqlMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PurchaseOrder",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Invoice",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Bill",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "PurchaseOrder");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Bill");
        }
    }
}
