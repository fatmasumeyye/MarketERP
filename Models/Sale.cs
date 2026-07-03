using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("sales")]
    public class Sale
    {
        public const string ActiveStatus = "Aktif";
        public const string CancelledStatus = "İptal Edildi";

        [Column("id")]
        public int Id { get; set; }

        [Column("customer_id")]
        public int? CustomerId { get; set; }

        [Column("employee_id")]
        public int? EmployeeId { get; set; }

        [Column("sale_date")]
        public DateTime SaleDate { get; set; }

        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [Column("payment_type")]
        public string PaymentType { get; set; } = "Nakit";

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(30)]
        [Column("status")]
        public string Status { get; set; } = ActiveStatus;

        [System.ComponentModel.DataAnnotations.MaxLength(500)]
        [Column("cancellation_reason")]
        public string? CancellationReason { get; set; }

        [Column("cancelled_at")]
        public DateTime? CancelledAt { get; set; }

        [Column("cancelled_by_employee_id")]
        public int? CancelledByEmployeeId { get; set; }

        public Customer? Customer { get; set; }

        public Employee? Employee { get; set; }

        public Employee? CancelledByEmployee { get; set; }
    }
}
