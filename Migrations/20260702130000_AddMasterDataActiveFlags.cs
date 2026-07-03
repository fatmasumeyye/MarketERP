using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketERP.Migrations
{
    public partial class AddMasterDataActiveFlags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "customers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "products",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "suppliers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "is_active", table: "customers");
            migrationBuilder.DropColumn(name: "is_active", table: "products");
            migrationBuilder.DropColumn(name: "is_active", table: "suppliers");
        }
    }
}
