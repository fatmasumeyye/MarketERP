using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("wholesale_sale_request_items")]
    public class WholesaleSaleRequestItem
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("wholesale_sale_request_id")]
        public int WholesaleSaleRequestId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("unit_price")]
        public decimal UnitPrice { get; set; }

        [Column("subtotal")]
        public decimal Subtotal { get; set; }

        public WholesaleSaleRequest? WholesaleSaleRequest { get; set; }

        public Product? Product { get; set; }
    }
}