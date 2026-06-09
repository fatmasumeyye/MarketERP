using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("categories")]
    public class Category
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("parent_category_id")]
        public int? ParentCategoryId { get; set; }

        [Column("default_vat_rate")]
        public decimal? DefaultVatRate { get; set; }

        public Category? ParentCategory { get; set; }

        public ICollection<Category> SubCategories { get; set; } = new List<Category>();

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}