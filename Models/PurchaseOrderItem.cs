using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("purchase_order_items")]
    public class PurchaseOrderItem
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("purchase_order_id")]
        public int PurchaseOrderId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("unit_price")]
        public decimal UnitPrice { get; set; }

        [Column("subtotal")]
        public decimal Subtotal { get; set; }

        [Column("received_quantity")]
        public int ReceivedQuantity { get; set; }

        public PurchaseOrder? PurchaseOrder { get; set; }

        public Product? Product { get; set; }
    }
}