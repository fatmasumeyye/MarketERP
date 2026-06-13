namespace MarketERP.Models;

public class EmployeeShiftScheduleViewModel
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public bool CanManageShifts { get; set; }
    public bool IsOwnShiftView { get; set; }
    public List<DateTime> WeekDays { get; set; } = [];
    public List<EmployeeShiftScheduleRowViewModel> Rows { get; set; } = [];
    public List<EmployeeShiftTypeOption> ShiftTypes { get; set; } = [];
    public Dictionary<DateTime, int> DailyEmployeeCounts { get; set; } = [];
}

public class EmployeeShiftScheduleRowViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? Position { get; set; }
    public Dictionary<DateTime, List<EmployeeShiftDisplayItem>> ShiftsByDay { get; set; } = [];
    public decimal WeeklyHours { get; set; }
}

public class EmployeeShiftDisplayItem
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime ShiftDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string ShiftTypeValue { get; set; } = string.Empty;
    public string ShiftLabel { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public bool IsLeave { get; set; }
}

public class EmployeeShiftTypeOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string DescriptionValue { get; set; } = string.Empty;
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public bool IsLeave { get; set; }
    public bool IsEmpty { get; set; }

    public string TimeRange => StartTime.HasValue && EndTime.HasValue && !IsLeave
        ? $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}"
        : string.Empty;
}
