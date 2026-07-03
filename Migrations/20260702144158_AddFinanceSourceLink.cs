using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketERP.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceSourceLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "kaynak_id",
                table: "finans_hareketleri",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "kaynak_no",
                table: "finans_hareketleri",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "kaynak_tipi",
                table: "finans_hareketleri",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "otomatik_mi",
                table: "finans_hareketleri",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_finans_hareketleri_kaynak_tipi_kaynak_id",
                table: "finans_hareketleri",
                columns: new[] { "kaynak_tipi", "kaynak_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_finans_hareketleri_kaynak_tipi_kaynak_id",
                table: "finans_hareketleri");

            migrationBuilder.DropColumn(
                name: "kaynak_id",
                table: "finans_hareketleri");

            migrationBuilder.DropColumn(
                name: "kaynak_no",
                table: "finans_hareketleri");

            migrationBuilder.DropColumn(
                name: "kaynak_tipi",
                table: "finans_hareketleri");

            migrationBuilder.DropColumn(
                name: "otomatik_mi",
                table: "finans_hareketleri");
        }
    }
}
