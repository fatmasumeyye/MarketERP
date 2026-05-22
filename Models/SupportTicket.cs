using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models
{
    [Table("support_tickets")]
    public class SupportTicket
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("customer_id")]
        public int? CustomerId { get; set; }

        [Column("subject")]
        public string Subject { get; set; }

        [Column("message")]
        public string Message { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public Customer Customer { get; set; }
    }
}