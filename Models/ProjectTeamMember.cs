using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models;

[Table("project_team_members")]
public class ProjectTeamMember
{
    [Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [Column("role")]
    public string Role { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Range(0, 10000)]
    [Column("estimated_work_hours")]
    public decimal EstimatedWorkHours { get; set; }

    [Range(0, 10000)]
    [Column("actual_work_hours")]
    public decimal ActualWorkHours { get; set; }

    public ICollection<ProjectTask> AssignedTasks { get; set; } = new List<ProjectTask>();
}
