using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("purchase_orders")]
    public class PurchaseOrder
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("supplier_id")]
        public int SupplierId { get; set; }

        [Column("employee_id")]
        public int? EmployeeId { get; set; }

        [Column("order_date")]
        public DateTime OrderDate { get; set; }

        [Column("approved_at")]
        public DateTime? ApprovedAt { get; set; }

        [Column("checked_at")]
        public DateTime? CheckedAt { get; set; }

        [Column("received_at")]
        public DateTime? ReceivedAt { get; set; }

        [Column("status")]
        public string Status { get; set; } = "Taslak";

        [Column("note")]
        public string? Note { get; set; }

        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public Supplier? Supplier { get; set; }

        public Employee? Employee { get; set; }

        public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }
}