using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("sales")]
    public class Sale
    {
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

        public Customer? Customer { get; set; }

        public Employee? Employee { get; set; }
    }
}