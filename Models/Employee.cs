using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("employees")]
    public class Employee
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("full_name")]
        public string FullName { get; set; }

        [Column("phone")]
        public string Phone { get; set; }

        [Column("position")]
        public string Position { get; set; }

        [Column("salary")]
        public decimal? Salary { get; set; }

        [Column("hire_date")]
        public DateTime? HireDate { get; set; }

        // YENİ ALANLAR

        [Column("email")]
        public string? Email { get; set; }

        [Column("username")]
        public string? Username { get; set; }

        [Column("password")]
        public string? Password { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation

        public ICollection<UserRole>? UserRoles { get; set; }
    }
}