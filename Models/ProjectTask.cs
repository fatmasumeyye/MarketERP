using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models;

[Table("project_tasks")]
public class ProjectTask
{
    [Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1500)]
    [Column("description")]
    public string? Description { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Column("duration_days")]
    public int DurationDays { get; set; }

    [Range(0, 100)]
    [Column("progress_percent")]
    public int ProgressPercent { get; set; }

    [Required, MaxLength(50)]
    [Column("status")]
    public string Status { get; set; } = "Planlandı";

    [Required, MaxLength(30)]
    [Column("priority")]
    public string Priority { get; set; } = "Orta";

    [Column("budget")]
    public decimal Budget { get; set; }

    [Column("cost")]
    public decimal Cost { get; set; }

    [Column("assigned_member_id")]
    public int? AssignedMemberId { get; set; }

    [Column("depends_on_task_id")]
    public int? DependsOnTaskId { get; set; }

    [Column("is_critical")]
    public bool IsCritical { get; set; }

    [Column("slack_days")]
    public int? SlackDays { get; set; }

    public ProjectTeamMember? AssignedMember { get; set; }

    public ProjectTask? DependsOnTask { get; set; }

    public ICollection<ProjectTask> DependentTasks { get; set; } = new List<ProjectTask>();
}
