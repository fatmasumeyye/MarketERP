using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("categories")]
    public class Category
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}