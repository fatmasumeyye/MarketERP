using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("suppliers")]
    public class Supplier
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("company_name")]
        public string CompanyName { get; set; }

        [Column("phone")]
        public string Phone { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("discount_rate")]
        public decimal DiscountRate { get; set; }

        public ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();

        public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    }
}