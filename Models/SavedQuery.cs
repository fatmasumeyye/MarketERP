using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("saved_queries")]
    public class SavedQuery
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("sql_query")]
        public string SqlQuery { get; set; }
    }
}