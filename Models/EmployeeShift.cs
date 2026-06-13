using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("employee_shifts")]
    public class EmployeeShift
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("shift_date")]
        public DateTime ShiftDate { get; set; }

        [Column("start_time")]
        public TimeSpan StartTime { get; set; }

        [Column("end_time")]
        public TimeSpan EndTime { get; set; }

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        public Employee? Employee { get; set; }
    }
}
