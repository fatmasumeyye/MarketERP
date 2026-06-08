using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("product_suppliers")]
    public class ProductSupplier
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("supplier_id")]
        public int SupplierId { get; set; }

        [Column("purchase_price")]
        public decimal PurchasePrice { get; set; }

        [Column("is_default")]
        public bool IsDefault { get; set; }

        [Column("min_order_quantity")]
        public int MinOrderQuantity { get; set; } = 1;

        [Column("lead_time_days")]
        public int? LeadTimeDays { get; set; }

        public Product? Product { get; set; }

        public Supplier? Supplier { get; set; }
    }
}