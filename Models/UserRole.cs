using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("user_roles")]
    public class UserRole
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("role_id")]
        public int RoleId { get; set; }

        public Employee Employee { get; set; }

        public Role Role { get; set; }
    }
}