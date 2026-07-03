using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketERP.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleCancellationAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason",
                table: "sales",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "cancelled_at",
                table: "sales",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cancelled_by_employee_id",
                table: "sales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "sales",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Aktif")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_sales_cancelled_by_employee_id",
                table: "sales",
                column: "cancelled_by_employee_id");

            migrationBuilder.AddForeignKey(
                name: "FK_sales_employees_cancelled_by_employee_id",
                table: "sales",
                column: "cancelled_by_employee_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sales_employees_cancelled_by_employee_id",
                table: "sales");

            migrationBuilder.DropIndex(
                name: "IX_sales_cancelled_by_employee_id",
                table: "sales");

            migrationBuilder.DropColumn(
                name: "cancellation_reason",
                table: "sales");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                table: "sales");

            migrationBuilder.DropColumn(
                name: "cancelled_by_employee_id",
                table: "sales");

            migrationBuilder.DropColumn(
                name: "status",
                table: "sales");
        }
    }
}
