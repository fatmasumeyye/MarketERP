using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("employee_leaves")]
    public class EmployeeLeave
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("leave_reason")]
        public string LeaveReason { get; set; }

        [Column("status")]
        public string Status { get; set; }

        public Employee Employee { get; set; }

        [NotMapped]
        public int RequestedDays { get; set; }

        [NotMapped]
        public int AnnualLeaveRight { get; set; }

        [NotMapped]
        public int UsedLeaveDays { get; set; }

        [NotMapped]
        public int PendingLeaveDays { get; set; }

        [NotMapped]
        public int RemainingLeaveDays { get; set; }

        [NotMapped]
        public int SeniorityYear { get; set; }
    }
}