using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("customers")]
    public class Customer
    {
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Müşteri adı zorunludur.")]
        [Column("full_name")]
        public string FullName { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Range(0, 100, ErrorMessage = "İskonto oranı 0 ile 100 arasında olmalıdır.")]
        [Column("discount_rate", TypeName = "decimal(5,2)")]
        public decimal DiscountRate { get; set; } = 0;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
