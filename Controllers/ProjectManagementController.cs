using MarketERP.Data;
using MarketERP.Helpers;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace MarketERP.Controllers;

[PermissionAuthorize("role.manage")]
public class ProjectManagementController : Controller
{
    private readonly AppDbContext _context;

    public ProjectManagementController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        var project = await GetProjectAsync();
        var tasks = await _context.ProjectTasks.AsNoTracking().ToListAsync();
        var progress = tasks.Count == 0
            ? project.ProgressPercent
            : (int)Math.Round(tasks.Average(t => t.ProgressPercent));

        return View(new ProjectManagementDashboardViewModel
        {
            Project = project,
            TotalTasks = tasks.Count,
            CompletedTasks = tasks.Count(IsCompleted),
            ActiveTasks = tasks.Count(t => !IsCompleted(t) && !IsPlanned(t)),
            DelayedTasks = tasks.Count(t => !IsCompleted(t) && t.EndDate.Date < DateTime.Today),
            ProgressPercent = progress,
            TeamMemberCount = await _context.ProjectTeamMembers.CountAsync(),
            TotalTaskCost = tasks.Sum(t => t.Cost)
        });
    }

    [HttpGet]
    public async Task<IActionResult> Gantt()
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        return View(await BuildGanttViewModelAsync(groupByMember: false));
    }

    [HttpGet]
    public async Task<IActionResult> GanttByMember()
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        return View(await BuildGanttViewModelAsync(groupByMember: true));
    }

    [HttpGet]
    public async Task<IActionResult> CriticalPath()
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        return View(await BuildCriticalPathViewModelAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Reports()
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        return View(await BuildReportsViewModelAsync());
    }

    [HttpGet]
    public async Task<IActionResult> ExportReportsCsv()
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        var model = await BuildReportsViewModelAsync();
        var csv = new StringBuilder();
        csv.AppendLine("PROJE RAPORU");
        csv.AppendLine($"Proje;{EscapeCsv(model.Project.Name)}");
        csv.AppendLine($"Tamamlanma;{model.ProgressPercent}%");
        csv.AppendLine($"Kalan;{model.RemainingPercent}%");
        csv.AppendLine();
        csv.AppendLine("GÖREV ÖZETİ");
        csv.AppendLine("Toplam;Tamamlanan;Devam Eden;Geciken;Planlanan;Kritik;Geciken Kritik");
        csv.AppendLine($"{model.TotalTasks};{model.CompletedTasks};{model.ActiveTasks};{model.DelayedTasks};{model.PlannedTasks};{model.CriticalTaskCount};{model.DelayedCriticalTaskCount}");
        csv.AppendLine();
        csv.AppendLine("BÜTÇE ÖZETİ");
        csv.AppendLine("Toplam Bütçe;Toplam Gelir;Toplam Gider;Net Bütçe;Görev Maliyetleri");
        csv.AppendLine($"{model.Project.Budget:0.00};{model.Project.Income:0.00};{model.Project.Expense:0.00};{model.Project.Income - model.Project.Expense:0.00};{model.TotalTaskCost:0.00}");
        csv.AppendLine();
        csv.AppendLine("EKİP RAPORU");
        csv.AppendLine("Ekip Üyesi;Rol;Atanan Görev;Ortalama İlerleme;Tahmini Saat;Gerçek Saat");
        foreach (var row in model.TeamRows)
        {
            csv.AppendLine(
                $"{EscapeCsv(row.Member.FullName)};{EscapeCsv(row.Member.Role)};{row.AssignedTaskCount};{row.AverageProgress}%;{row.EstimatedWorkHours:0.##};{row.ActualWorkHours:0.##}");
        }

        csv.AppendLine();
        csv.AppendLine("GÖREV DETAYI");
        csv.AppendLine("Görev;Durum;Öncelik;Sorumlu;Başlangıç;Bitiş;İlerleme;Bütçe;Maliyet");
        foreach (var task in model.Tasks)
        {
            csv.AppendLine(
                $"{EscapeCsv(task.Title)};{EscapeCsv(task.Status)};{EscapeCsv(task.Priority)};{EscapeCsv(task.AssignedMember?.FullName ?? "Atanmamış")};{task.StartDate:dd.MM.yyyy};{task.EndDate:dd.MM.yyyy};{task.ProgressPercent}%;{task.Budget:0.00};{task.Cost:0.00}");
        }

        var bytes = Encoding.UTF8.GetPreamble()
            .Concat(Encoding.UTF8.GetBytes(csv.ToString()))
            .ToArray();
        return File(bytes, "text/csv; charset=utf-8", $"proje-raporu-{DateTime.Today:yyyy-MM-dd}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> Tasks(int? editId)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        return View(await BuildTasksViewModelAsync(editId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTask(
        [Bind(Prefix = "FormTask")] ProjectTask formTask)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        ValidateTask(formTask);
        if (!ModelState.IsValid)
        {
            var model = await BuildTasksViewModelAsync(formTask.Id);
            model.FormTask = formTask;
            return View("Tasks", model);
        }

        ProjectTask entity;
        if (formTask.Id == 0)
        {
            entity = new ProjectTask();
            _context.ProjectTasks.Add(entity);
        }
        else
        {
            entity = await _context.ProjectTasks.FindAsync(formTask.Id)
                ?? throw new InvalidOperationException("Düzenlenecek görev bulunamadı.");
        }

        CopyTaskValues(formTask, entity);
        await _context.SaveChangesAsync();
        await RefreshProjectProgressAsync();

        TempData["Success"] = formTask.Id == 0
            ? "Proje görevi başarıyla eklendi."
            : "Proje görevi başarıyla güncellendi.";
        return RedirectToAction(nameof(Tasks));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTask(int id)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        var task = await _context.ProjectTasks.FindAsync(id);
        if (task == null)
        {
            TempData["Error"] = "Silinecek görev bulunamadı.";
            return RedirectToAction(nameof(Tasks));
        }

        var dependents = await _context.ProjectTasks
            .Where(t => t.DependsOnTaskId == id)
            .ToListAsync();
        foreach (var dependent in dependents)
        {
            dependent.DependsOnTaskId = null;
        }

        _context.ProjectTasks.Remove(task);
        await _context.SaveChangesAsync();
        await RefreshProjectProgressAsync();
        TempData["Success"] = "Proje görevi silindi.";
        return RedirectToAction(nameof(Tasks));
    }

    [HttpGet]
    public async Task<IActionResult> Team(int? editId)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        return View(await BuildTeamViewModelAsync(editId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTeamMember(
        [Bind(Prefix = "FormMember")] ProjectTeamMember formMember)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        var emailExists = await _context.ProjectTeamMembers.AnyAsync(m =>
            m.Email == formMember.Email && m.Id != formMember.Id);
        if (emailExists)
        {
            ModelState.AddModelError(nameof(formMember.Email), "Bu e-posta adresi zaten kayıtlı.");
        }

        if (!ModelState.IsValid)
        {
            var model = await BuildTeamViewModelAsync(formMember.Id);
            model.FormMember = formMember;
            return View("Team", model);
        }

        ProjectTeamMember entity;
        if (formMember.Id == 0)
        {
            entity = new ProjectTeamMember();
            _context.ProjectTeamMembers.Add(entity);
        }
        else
        {
            entity = await _context.ProjectTeamMembers.FindAsync(formMember.Id)
                ?? throw new InvalidOperationException("Düzenlenecek ekip üyesi bulunamadı.");
        }

        entity.FullName = formMember.FullName.Trim();
        entity.Role = formMember.Role.Trim();
        entity.Email = formMember.Email.Trim();
        entity.EstimatedWorkHours = formMember.EstimatedWorkHours;
        entity.ActualWorkHours = formMember.ActualWorkHours;

        await _context.SaveChangesAsync();
        TempData["Success"] = formMember.Id == 0
            ? "Ekip üyesi başarıyla eklendi."
            : "Ekip üyesi başarıyla güncellendi.";
        return RedirectToAction(nameof(Team));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTeamMember(int id)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        var member = await _context.ProjectTeamMembers.FindAsync(id);
        if (member == null)
        {
            TempData["Error"] = "Silinecek ekip üyesi bulunamadı.";
            return RedirectToAction(nameof(Team));
        }

        var assignedTasks = await _context.ProjectTasks
            .Where(t => t.AssignedMemberId == id)
            .ToListAsync();
        foreach (var task in assignedTasks)
        {
            task.AssignedMemberId = null;
        }

        _context.ProjectTeamMembers.Remove(member);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Ekip üyesi silindi; görev atamaları boş bırakıldı.";
        return RedirectToAction(nameof(Team));
    }

    [HttpGet]
    public async Task<IActionResult> Budget()
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        var project = await GetProjectAsync();
        var tasks = await _context.ProjectTasks
            .AsNoTracking()
            .Include(t => t.AssignedMember)
            .OrderByDescending(t => t.Cost)
            .ToListAsync();
        var members = await _context.ProjectTeamMembers
            .AsNoTracking()
            .OrderBy(m => m.FullName)
            .ToListAsync();

        return View(new ProjectBudgetViewModel
        {
            Project = project,
            Tasks = tasks,
            Members = members,
            TotalTaskBudget = tasks.Sum(t => t.Budget),
            TotalTaskCost = tasks.Sum(t => t.Cost)
        });
    }

    private async Task<ProjectTasksViewModel> BuildTasksViewModelAsync(int? editId)
    {
        var tasks = await _context.ProjectTasks
            .AsNoTracking()
            .Include(t => t.AssignedMember)
            .Include(t => t.DependsOnTask)
            .OrderBy(t => t.StartDate)
            .ThenBy(t => t.Title)
            .ToListAsync();
        var members = await _context.ProjectTeamMembers
            .AsNoTracking()
            .OrderBy(m => m.FullName)
            .ToListAsync();
        var formTask = editId.HasValue
            ? await _context.ProjectTasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == editId)
            : null;

        return new ProjectTasksViewModel
        {
            Tasks = tasks,
            TeamMembers = members,
            FormTask = formTask ?? new ProjectTask
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7),
                Status = "Planlandı",
                Priority = "Orta"
            }
        };
    }

    private async Task<ProjectTeamViewModel> BuildTeamViewModelAsync(int? editId)
    {
        var members = await _context.ProjectTeamMembers
            .AsNoTracking()
            .OrderBy(m => m.FullName)
            .ToListAsync();
        var formMember = editId.HasValue
            ? members.FirstOrDefault(m => m.Id == editId)
            : null;

        return new ProjectTeamViewModel
        {
            Members = members,
            FormMember = formMember ?? new ProjectTeamMember()
        };
    }

    private async Task<ProjectGanttViewModel> BuildGanttViewModelAsync(bool groupByMember)
    {
        var project = await GetProjectAsync();
        var allTasks = await _context.ProjectTasks
            .AsNoTracking()
            .Include(t => t.AssignedMember)
            .Include(t => t.DependsOnTask)
            .OrderBy(t => t.StartDate)
            .ThenBy(t => t.Title)
            .ToListAsync();
        var validTasks = allTasks
            .Where(t => t.StartDate != default
                && t.EndDate != default
                && t.StartDate.Date <= t.EndDate.Date)
            .ToList();

        var timelineStart = validTasks.Count > 0
            ? validTasks.Min(t => t.StartDate.Date)
            : project.StartDate.Date;
        var timelineEnd = validTasks.Count > 0
            ? validTasks.Max(t => t.EndDate.Date)
            : project.EndDate.Date;

        if (timelineStart == default)
        {
            timelineStart = DateTime.Today;
        }
        if (timelineEnd < timelineStart)
        {
            timelineEnd = timelineStart;
        }

        var totalDays = Math.Max(1, (timelineEnd - timelineStart).Days + 1);
        var rows = validTasks
            .Select(task => CreateGanttRow(task, timelineStart, totalDays))
            .ToList();
        var model = new ProjectGanttViewModel
        {
            Project = project,
            TimelineStart = timelineStart,
            TimelineEnd = timelineEnd,
            TotalDays = totalDays,
            InvalidTaskCount = allTasks.Count - validTasks.Count,
            WeekMarkers = Enumerable.Range(0, (int)Math.Ceiling(totalDays / 7m))
                .Select(week => timelineStart.AddDays(week * 7))
                .ToList(),
            Rows = rows
        };

        if (groupByMember)
        {
            model.MemberGroups = rows
                .GroupBy(row => row.Task.AssignedMemberId)
                .Select(group => new ProjectGanttMemberGroupViewModel
                {
                    MemberName = group.First().Task.AssignedMember?.FullName ?? "Atanmamış",
                    MemberRole = group.First().Task.AssignedMember?.Role ?? "Sorumlu seçilmemiş",
                    Rows = group.OrderBy(row => row.Task.StartDate).ToList()
                })
                .OrderBy(group => group.MemberName == "Atanmamış" ? 1 : 0)
                .ThenBy(group => group.MemberName)
                .ToList();
        }

        return model;
    }

    private async Task<ProjectCriticalPathViewModel> BuildCriticalPathViewModelAsync()
    {
        var project = await GetProjectAsync();
        var allTasks = await _context.ProjectTasks
            .AsNoTracking()
            .Include(t => t.AssignedMember)
            .Include(t => t.DependsOnTask)
            .OrderBy(t => t.StartDate)
            .ThenBy(t => t.Title)
            .ToListAsync();
        var validTasks = allTasks.Where(IsValidScheduleTask).ToList();
        var taskById = validTasks.ToDictionary(t => t.Id);
        var longestChain = new List<ProjectTask>();
        var longestDuration = 0;
        var hasCycle = false;

        foreach (var endTask in validTasks)
        {
            var chain = new List<ProjectTask>();
            var visited = new HashSet<int>();
            ProjectTask? current = endTask;
            var chainHasCycle = false;

            while (current != null)
            {
                if (!visited.Add(current.Id))
                {
                    hasCycle = true;
                    chainHasCycle = true;
                    break;
                }

                chain.Add(current);
                current = current.DependsOnTaskId.HasValue
                    && taskById.TryGetValue(current.DependsOnTaskId.Value, out var predecessor)
                        ? predecessor
                        : null;
            }

            if (chainHasCycle)
            {
                continue;
            }

            chain.Reverse();
            var chainDuration = chain.Sum(GetTaskDurationDays);
            if (chainDuration > longestDuration)
            {
                longestDuration = chainDuration;
                longestChain = chain;
            }
        }

        var criticalIds = longestChain.Select(t => t.Id).ToHashSet();
        var projectFinish = validTasks.Count > 0
            ? validTasks.Max(t => t.EndDate.Date)
            : project.EndDate.Date;
        if (project.EndDate != default && project.EndDate.Date > projectFinish)
        {
            projectFinish = project.EndDate.Date;
        }
        if (projectFinish == default)
        {
            projectFinish = DateTime.Today;
        }

        var criticalRows = longestChain
            .Select((task, index) => new ProjectCriticalTaskViewModel
            {
                Task = task,
                Sequence = index + 1,
                DurationDays = GetTaskDurationDays(task),
                SlackDays = 0,
                IsCritical = true
            })
            .ToList();
        var nonCriticalRows = validTasks
            .Where(task => !criticalIds.Contains(task.Id))
            .Select(task => new ProjectCriticalTaskViewModel
            {
                Task = task,
                DurationDays = GetTaskDurationDays(task),
                SlackDays = CalculateApproximateSlackDays(task, validTasks, projectFinish),
                IsCritical = false
            })
            .OrderBy(row => row.SlackDays)
            .ThenBy(row => row.Task.StartDate)
            .ToList();

        // Görevler takvimde paralel ilerleyebildiği için üst kartta görev sürelerinin
        // basit toplamını değil, ilk kritik başlangıç ile tahmini bitiş arasındaki
        // kapsayıcı takvim süresini gösteriyoruz.
        var estimatedFinish = projectFinish;
        var criticalCalendarStart = longestChain.Count > 0
            ? longestChain.Min(task => task.StartDate.Date)
            : project.StartDate.Date;
        if (criticalCalendarStart == default || criticalCalendarStart > estimatedFinish)
        {
            criticalCalendarStart = estimatedFinish;
        }
        var criticalCalendarDuration = (estimatedFinish - criticalCalendarStart).Days + 1;

        return new ProjectCriticalPathViewModel
        {
            Project = project,
            CriticalTasks = criticalRows,
            NonCriticalTasks = nonCriticalRows,
            TotalCriticalDurationDays = Math.Max(1, criticalCalendarDuration),
            EstimatedFinishDate = estimatedFinish,
            HasDependencyCycle = hasCycle,
            InvalidTaskCount = allTasks.Count - validTasks.Count
        };
    }

    private async Task<ProjectReportsViewModel> BuildReportsViewModelAsync()
    {
        var project = await GetProjectAsync();
        var tasks = await _context.ProjectTasks
            .AsNoTracking()
            .Include(t => t.AssignedMember)
            .OrderBy(t => t.StartDate)
            .ThenBy(t => t.Title)
            .ToListAsync();
        var members = await _context.ProjectTeamMembers
            .AsNoTracking()
            .OrderBy(m => m.FullName)
            .ToListAsync();
        var criticalPath = await BuildCriticalPathViewModelAsync();
        var criticalIds = criticalPath.CriticalTasks.Select(row => row.Task.Id).ToHashSet();
        var progress = tasks.Count == 0
            ? project.ProgressPercent
            : (int)Math.Round(tasks.Average(t => t.ProgressPercent));

        return new ProjectReportsViewModel
        {
            Project = project,
            TotalTasks = tasks.Count,
            CompletedTasks = tasks.Count(IsCompleted),
            ActiveTasks = tasks.Count(t => !IsCompleted(t)
                && !IsPlanned(t)
                && t.EndDate.Date >= DateTime.Today),
            DelayedTasks = tasks.Count(t => !IsCompleted(t) && t.EndDate.Date < DateTime.Today),
            PlannedTasks = tasks.Count(IsPlanned),
            ProgressPercent = progress,
            RemainingPercent = Math.Max(0, 100 - progress),
            CriticalTaskCount = criticalIds.Count,
            DelayedCriticalTaskCount = tasks.Count(t => criticalIds.Contains(t.Id)
                && !IsCompleted(t)
                && t.EndDate.Date < DateTime.Today),
            TotalTaskBudget = tasks.Sum(t => t.Budget),
            TotalTaskCost = tasks.Sum(t => t.Cost),
            Tasks = tasks,
            TeamRows = members.Select(member =>
            {
                var memberTasks = tasks.Where(t => t.AssignedMemberId == member.Id).ToList();
                return new ProjectTeamReportRowViewModel
                {
                    Member = member,
                    AssignedTaskCount = memberTasks.Count,
                    AverageProgress = memberTasks.Count == 0
                        ? 0
                        : (int)Math.Round(memberTasks.Average(t => t.ProgressPercent)),
                    EstimatedWorkHours = member.EstimatedWorkHours,
                    ActualWorkHours = member.ActualWorkHours
                };
            }).ToList()
        };
    }

    private static int CalculateApproximateSlackDays(
        ProjectTask task,
        IReadOnlyCollection<ProjectTask> tasks,
        DateTime projectFinish)
    {
        var dependentStartDates = tasks
            .Where(candidate => candidate.DependsOnTaskId == task.Id
                && candidate.StartDate != default)
            .Select(candidate => candidate.StartDate.Date)
            .ToList();
        var latestAllowedFinish = dependentStartDates.Count > 0
            ? dependentStartDates.Min().AddDays(-1)
            : projectFinish;

        return Math.Max(0, (latestAllowedFinish - task.EndDate.Date).Days);
    }

    private static bool IsValidScheduleTask(ProjectTask task)
    {
        return task.StartDate != default
            && task.EndDate != default
            && task.StartDate.Date <= task.EndDate.Date;
    }

    private static int GetTaskDurationDays(ProjectTask task)
    {
        return task.DurationDays > 0
            ? task.DurationDays
            : Math.Max(1, (task.EndDate.Date - task.StartDate.Date).Days + 1);
    }

    private static string EscapeCsv(string? value)
    {
        var safeValue = value ?? string.Empty;
        return safeValue.Contains(';') || safeValue.Contains('"') || safeValue.Contains('\n')
            ? $"\"{safeValue.Replace("\"", "\"\"")}\""
            : safeValue;
    }

    private static ProjectGanttRowViewModel CreateGanttRow(
        ProjectTask task,
        DateTime timelineStart,
        int totalDays)
    {
        var duration = Math.Max(1, (task.EndDate.Date - task.StartDate.Date).Days + 1);
        var left = Math.Max(0, (task.StartDate.Date - timelineStart).Days);
        var delayed = !IsCompleted(task) && task.EndDate.Date < DateTime.Today;
        var statusClass = delayed
            ? "bg-danger"
            : IsCompleted(task)
                ? "bg-success"
                : IsPlanned(task)
                    ? "bg-secondary"
                    : "bg-primary";

        return new ProjectGanttRowViewModel
        {
            Task = task,
            LeftPercent = Math.Round(left * 100m / totalDays, 4),
            WidthPercent = Math.Max(0.8m, Math.Round(duration * 100m / totalDays, 4)),
            IsDelayed = delayed,
            StatusCssClass = statusClass
        };
    }

    private async Task<ProjectModule> GetProjectAsync()
    {
        return await _context.ProjectModules.AsNoTracking().OrderBy(p => p.Id).FirstOrDefaultAsync()
            ?? new ProjectModule
            {
                Name = "MarketERP",
                Description = "Market operasyonlarının uçtan uca yönetildiği ERP geliştirme projesi.",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                Status = "Planlandı"
            };
    }

    private void ValidateTask(ProjectTask task)
    {
        if (task.EndDate.Date < task.StartDate.Date)
        {
            ModelState.AddModelError(nameof(task.EndDate), "Bitiş tarihi başlangıç tarihinden önce olamaz.");
        }

        if (task.Id > 0 && task.DependsOnTaskId == task.Id)
        {
            ModelState.AddModelError(nameof(task.DependsOnTaskId), "Görev kendisine bağımlı olamaz.");
        }

        if (task.AssignedMemberId.HasValue
            && !_context.ProjectTeamMembers.Any(m => m.Id == task.AssignedMemberId.Value))
        {
            ModelState.AddModelError(nameof(task.AssignedMemberId), "Seçilen ekip üyesi bulunamadı.");
        }

        if (task.DependsOnTaskId.HasValue
            && !_context.ProjectTasks.Any(t => t.Id == task.DependsOnTaskId.Value))
        {
            ModelState.AddModelError(nameof(task.DependsOnTaskId), "Bağımlı görev bulunamadı.");
        }
    }

    private static void CopyTaskValues(ProjectTask source, ProjectTask target)
    {
        target.Title = source.Title.Trim();
        target.Description = source.Description?.Trim();
        target.StartDate = source.StartDate.Date;
        target.EndDate = source.EndDate.Date;
        target.DurationDays = Math.Max(1, (source.EndDate.Date - source.StartDate.Date).Days + 1);
        target.ProgressPercent = source.ProgressPercent;
        target.Status = source.Status.Trim();
        target.Priority = source.Priority.Trim();
        target.Budget = source.Budget;
        target.Cost = source.Cost;
        target.AssignedMemberId = source.AssignedMemberId;
        target.DependsOnTaskId = source.DependsOnTaskId;
        target.IsCritical = source.IsCritical;
        target.SlackDays = source.SlackDays;
    }

    private async Task RefreshProjectProgressAsync()
    {
        var project = await _context.ProjectModules.OrderBy(p => p.Id).FirstOrDefaultAsync();
        if (project == null)
        {
            return;
        }

        var progressValues = await _context.ProjectTasks.Select(t => t.ProgressPercent).ToListAsync();
        project.ProgressPercent = progressValues.Count == 0
            ? 0
            : (int)Math.Round(progressValues.Average());
        project.Status = project.ProgressPercent >= 100 ? "Tamamlandı" : "Devam Ediyor";
        await _context.SaveChangesAsync();
    }

    private bool IsAdmin()
    {
        return (HttpContext.Session.GetString("Roles") ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains("Admin", StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsCompleted(ProjectTask task)
    {
        return task.ProgressPercent >= 100
            || string.Equals(task.Status, "Tamamlandı", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPlanned(ProjectTask task)
    {
        return string.Equals(task.Status, "Planlandı", StringComparison.OrdinalIgnoreCase);
    }
}
