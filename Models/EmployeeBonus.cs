using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("employee_bonuses")]
    public class EmployeeBonus
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("bonus_amount")]
        public decimal BonusAmount { get; set; }

        [Column("bonus_date")]
        public DateTime BonusDate { get; set; }

        [Column("description")]
        public string Description { get; set; }

        public Employee Employee { get; set; }
    }
}