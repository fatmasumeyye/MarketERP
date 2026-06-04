using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("return_requests")]
    public class ReturnRequest
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("sale_id")]
        public int SaleId { get; set; }

        [Column("sale_detail_id")]
        public int SaleDetailId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("reason")]
        public string Reason { get; set; } = string.Empty;

        [Column("status")]
        public string Status { get; set; } = "Beklemede";

        [Column("requested_at")]
        public DateTime RequestedAt { get; set; }

        [Column("reviewed_at")]
        public DateTime? ReviewedAt { get; set; }

        [Column("review_note")]
        public string? ReviewNote { get; set; }

        public Sale? Sale { get; set; }

        public SaleDetail? SaleDetail { get; set; }

        public Product? Product { get; set; }

        public Employee? Employee { get; set; }
    }
}