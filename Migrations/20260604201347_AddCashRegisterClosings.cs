using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketERP.Migrations
{
    /// <inheritdoc />
    public partial class AddCashRegisterClosings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cash_register_closings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    employee_id = table.Column<int>(type: "int", nullable: false),
                    closing_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    cash_sales_total = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    card_sales_total = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    total_sales_amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    declared_cash_amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    cash_difference = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    note = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    reviewed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    review_note = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cash_register_closings", x => x.id);
                    table.ForeignKey(
                        name: "FK_cash_register_closings_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_cash_register_closings_employee_id",
                table: "cash_register_closings",
                column: "employee_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cash_register_closings");
        }
    }
}
