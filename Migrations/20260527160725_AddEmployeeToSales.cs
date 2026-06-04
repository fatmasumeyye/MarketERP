using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketERP.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeToSales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "employee_id",
                table: "sales",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_sales_employee_id",
                table: "sales",
                column: "employee_id");

            migrationBuilder.AddForeignKey(
                name: "FK_sales_employees_employee_id",
                table: "sales",
                column: "employee_id",
                principalTable: "employees",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sales_employees_employee_id",
                table: "sales");

            migrationBuilder.DropIndex(
                name: "IX_sales_employee_id",
                table: "sales");

            migrationBuilder.DropColumn(
                name: "employee_id",
                table: "sales");
        }
    }
}
