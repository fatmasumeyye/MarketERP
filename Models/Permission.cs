using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("permissions")]
    public class Permission
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        public ICollection<RolePermission>? RolePermissions { get; set; }
    }
}