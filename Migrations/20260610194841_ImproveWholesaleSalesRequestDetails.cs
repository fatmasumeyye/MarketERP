using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketERP.Migrations
{
    /// <inheritdoc />
    public partial class ImproveWholesaleSalesRequestDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "delivery_address",
                table: "wholesale_sale_requests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "delivery_date",
                table: "wholesale_sale_requests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "offer_valid_until",
                table: "wholesale_sale_requests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_type",
                table: "wholesale_sale_requests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "delivery_address",
                table: "wholesale_sale_requests");

            migrationBuilder.DropColumn(
                name: "delivery_date",
                table: "wholesale_sale_requests");

            migrationBuilder.DropColumn(
                name: "offer_valid_until",
                table: "wholesale_sale_requests");

            migrationBuilder.DropColumn(
                name: "payment_type",
                table: "wholesale_sale_requests");
        }
    }
}
