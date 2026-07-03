using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models;

[Table("stock_movement_logs")]
public class StockMovement
{
    [Column("id")]
    public int Id { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Required, MaxLength(20)]
    [Column("movement_type")]
    public string MovementType { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    [Column("reason_type")]
    public string ReasonType { get; set; } = string.Empty;

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("previous_quantity")]
    public int PreviousQuantity { get; set; }

    [Column("new_quantity")]
    public int NewQuantity { get; set; }

    [Column("unit_cost")]
    public decimal? UnitCost { get; set; }

    [Column("movement_date")]
    public DateTime MovementDate { get; set; }

    [MaxLength(50)]
    [Column("source_type")]
    public string? SourceType { get; set; }

    [Column("source_id")]
    public int? SourceId { get; set; }

    [Column("source_line_id")]
    public int? SourceLineId { get; set; }

    [MaxLength(100)]
    [Column("source_no")]
    public string? SourceNo { get; set; }

    [MaxLength(1000)]
    [Column("description")]
    public string? Description { get; set; }

    [Column("created_by_employee_id")]
    public int? CreatedByEmployeeId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("reversal_of_movement_id")]
    public int? ReversalOfMovementId { get; set; }

    public Product? Product { get; set; }
    public Employee? CreatedByEmployee { get; set; }
    public StockMovement? ReversalOfMovement { get; set; }
}
