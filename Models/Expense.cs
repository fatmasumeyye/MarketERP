using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("expenses")]
    public class Expense
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("expense_date")]
        public DateTime ExpenseDate { get; set; }

        [Column("description")]
        public string Description { get; set; }
    }
}