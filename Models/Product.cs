using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("products")]
    public class Product
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("category_id")]
        public int? CategoryId { get; set; }

        [Column("supplier_id")]
        public int? SupplierId { get; set; }

        [Column("barcode")]
        public string Barcode { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("purchase_price")]
        public decimal PurchasePrice { get; set; }

        [Column("sale_price")]
        public decimal SalePrice { get; set; }

        [Column("stock_quantity")]
        public int StockQuantity { get; set; }

        [Column("critical_stock")]
        public int CriticalStock { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public Category Category { get; set; }
        public Supplier Supplier { get; set; }
    }
}