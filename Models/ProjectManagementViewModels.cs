namespace MarketERP.Models;

public class ProjectManagementDashboardViewModel
{
    public ProjectModule Project { get; set; } = new();
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int ActiveTasks { get; set; }
    public int DelayedTasks { get; set; }
    public int ProgressPercent { get; set; }
    public int TeamMemberCount { get; set; }
    public decimal TotalTaskCost { get; set; }
}

public class ProjectTasksViewModel
{
    public List<ProjectTask> Tasks { get; set; } = [];
    public List<ProjectTeamMember> TeamMembers { get; set; } = [];
    public ProjectTask FormTask { get; set; } = new();
    public bool IsEditing => FormTask.Id > 0;
}

public class ProjectTeamViewModel
{
    public List<ProjectTeamMember> Members { get; set; } = [];
    public ProjectTeamMember FormMember { get; set; } = new();
    public bool IsEditing => FormMember.Id > 0;
}

public class ProjectBudgetViewModel
{
    public ProjectModule Project { get; set; } = new();
    public List<ProjectTask> Tasks { get; set; } = [];
    public List<ProjectTeamMember> Members { get; set; } = [];
    public decimal TotalTaskBudget { get; set; }
    public decimal TotalTaskCost { get; set; }
}

public class ProjectGanttViewModel
{
    public ProjectModule Project { get; set; } = new();
    public DateTime TimelineStart { get; set; }
    public DateTime TimelineEnd { get; set; }
    public int TotalDays { get; set; }
    public int InvalidTaskCount { get; set; }
    public List<DateTime> WeekMarkers { get; set; } = [];
    public List<ProjectGanttRowViewModel> Rows { get; set; } = [];
    public List<ProjectGanttMemberGroupViewModel> MemberGroups { get; set; } = [];
}

public class ProjectGanttRowViewModel
{
    public ProjectTask Task { get; set; } = new();
    public decimal LeftPercent { get; set; }
    public decimal WidthPercent { get; set; }
    public bool IsDelayed { get; set; }
    public string StatusCssClass { get; set; } = "bg-secondary";
}

public class ProjectGanttMemberGroupViewModel
{
    public string MemberName { get; set; } = string.Empty;
    public string MemberRole { get; set; } = string.Empty;
    public List<ProjectGanttRowViewModel> Rows { get; set; } = [];
}

public class ProjectCriticalPathViewModel
{
    public ProjectModule Project { get; set; } = new();
    public List<ProjectCriticalTaskViewModel> CriticalTasks { get; set; } = [];
    public List<ProjectCriticalTaskViewModel> NonCriticalTasks { get; set; } = [];
    public int TotalCriticalDurationDays { get; set; }
    public DateTime EstimatedFinishDate { get; set; }
    public bool HasDependencyCycle { get; set; }
    public int InvalidTaskCount { get; set; }
}

public class ProjectCriticalTaskViewModel
{
    public ProjectTask Task { get; set; } = new();
    public int Sequence { get; set; }
    public int DurationDays { get; set; }
    public int SlackDays { get; set; }
    public bool IsCritical { get; set; }
}

public class ProjectReportsViewModel
{
    public ProjectModule Project { get; set; } = new();
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int ActiveTasks { get; set; }
    public int DelayedTasks { get; set; }
    public int PlannedTasks { get; set; }
    public int ProgressPercent { get; set; }
    public int RemainingPercent { get; set; }
    public int CriticalTaskCount { get; set; }
    public int DelayedCriticalTaskCount { get; set; }
    public decimal TotalTaskBudget { get; set; }
    public decimal TotalTaskCost { get; set; }
    public List<ProjectTeamReportRowViewModel> TeamRows { get; set; } = [];
    public List<ProjectTask> Tasks { get; set; } = [];
}

public class ProjectTeamReportRowViewModel
{
    public ProjectTeamMember Member { get; set; } = new();
    public int AssignedTaskCount { get; set; }
    public int AverageProgress { get; set; }
    public decimal EstimatedWorkHours { get; set; }
    public decimal ActualWorkHours { get; set; }
}
