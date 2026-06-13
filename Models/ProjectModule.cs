using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models;

[Table("project_modules")]
public class ProjectModule
{
    [Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    [Column("description")]
    public string? Description { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Column("budget")]
    public decimal Budget { get; set; }

    [Column("income")]
    public decimal Income { get; set; }

    [Column("expense")]
    public decimal Expense { get; set; }

    [Required, MaxLength(50)]
    [Column("status")]
    public string Status { get; set; } = "Planlandı";

    [Range(0, 100)]
    [Column("progress_percent")]
    public int ProgressPercent { get; set; }
}
