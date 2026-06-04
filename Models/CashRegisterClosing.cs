using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("cash_register_closings")]
    public class CashRegisterClosing
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("closing_date")]
        public DateTime ClosingDate { get; set; }

        [Column("cash_sales_total")]
        public decimal CashSalesTotal { get; set; }

        [Column("card_sales_total")]
        public decimal CardSalesTotal { get; set; }

        [Column("total_sales_amount")]
        public decimal TotalSalesAmount { get; set; }

        [Column("declared_cash_amount")]
        public decimal DeclaredCashAmount { get; set; }

        [Column("cash_difference")]
        public decimal CashDifference { get; set; }

        [Column("note")]
        public string? Note { get; set; }

        [Column("status")]
        public string Status { get; set; } = "Beklemede";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("reviewed_at")]
        public DateTime? ReviewedAt { get; set; }

        [Column("review_note")]
        public string? ReviewNote { get; set; }

        public Employee? Employee { get; set; }
    }
}