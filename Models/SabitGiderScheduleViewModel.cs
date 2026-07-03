namespace MarketERP.Models;

public class SabitGiderScheduleViewModel
{
    public bool ProcessedThisMonth { get; set; }
    public FinansHareketi? LastMovement { get; set; }
    public DateTime? NextDate { get; set; }
    public string NextDateLabel { get; set; } = "Tanımlanmadı";
}
