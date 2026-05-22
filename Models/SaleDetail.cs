using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("sale_details")]
    public class SaleDetail
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("sale_id")]
        public int SaleId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("unit_price")]
        public decimal UnitPrice { get; set; }

        [Column("subtotal")]
        public decimal Subtotal { get; set; }

        public Sale Sale { get; set; }
        public Product Product { get; set; }
    }
}