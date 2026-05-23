using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("roles")]
    public class Role
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        public ICollection<UserRole>? UserRoles { get; set; }

        public ICollection<RolePermission>? RolePermissions { get; set; }
    }
}