using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketERP.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMovementLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stock_movement_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    movement_type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reason_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    previous_quantity = table.Column<int>(type: "int", nullable: false),
                    new_quantity = table.Column<int>(type: "int", nullable: false),
                    unit_cost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    movement_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    source_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_id = table.Column<int>(type: "int", nullable: true),
                    source_line_id = table.Column<int>(type: "int", nullable: true),
                    source_no = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by_employee_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    reversal_of_movement_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_movement_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_stock_movement_logs_employees_created_by_employee_id",
                        column: x => x.created_by_employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_stock_movement_logs_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_movement_logs_stock_movement_logs_reversal_of_movement~",
                        column: x => x.reversal_of_movement_id,
                        principalTable: "stock_movement_logs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movement_logs_created_by_employee_id",
                table: "stock_movement_logs",
                column: "created_by_employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movement_logs_product_id_movement_date",
                table: "stock_movement_logs",
                columns: new[] { "product_id", "movement_date" });

            migrationBuilder.CreateIndex(
                name: "IX_stock_movement_logs_reversal_of_movement_id",
                table: "stock_movement_logs",
                column: "reversal_of_movement_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movement_logs_source_type_source_id_source_line_id",
                table: "stock_movement_logs",
                columns: new[] { "source_type", "source_id", "source_line_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_movement_logs");
        }
    }
}
