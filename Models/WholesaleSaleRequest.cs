using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("wholesale_sale_requests")]
    public class WholesaleSaleRequest
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Column("employee_id")]
        public int? EmployeeId { get; set; }

        [Column("request_date")]
        public DateTime RequestDate { get; set; }

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Column("delivery_date")]
        public DateTime? DeliveryDate { get; set; }

        [Column("offer_valid_until")]
        public DateTime? OfferValidUntil { get; set; }

        [Column("delivery_address")]
        public string? DeliveryAddress { get; set; }

        [Column("payment_type")]
        public string? PaymentType { get; set; }

        [Column("discount_rate")]
        public decimal DiscountRate { get; set; }

        [Column("subtotal_amount")]
        public decimal SubtotalAmount { get; set; }

        [Column("discount_amount")]
        public decimal DiscountAmount { get; set; }

        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [Column("status")]
        public string Status { get; set; } = "Onay Bekliyor";

        [Column("note")]
        public string? Note { get; set; }

        [Column("approved_at")]
        public DateTime? ApprovedAt { get; set; }

        [Column("rejected_at")]
        public DateTime? RejectedAt { get; set; }

        [Column("review_note")]
        public string? ReviewNote { get; set; }

        [Column("sale_id")]
        public int? SaleId { get; set; }

        public Customer? Customer { get; set; }

        public Employee? Employee { get; set; }

        public Sale? Sale { get; set; }

        public ICollection<WholesaleSaleRequestItem> Items { get; set; } = new List<WholesaleSaleRequestItem>();
    }
}