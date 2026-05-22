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
    }
}