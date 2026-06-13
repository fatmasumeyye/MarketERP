using MarketERP.Data;
using MarketERP.Helpers;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Security;
using System.Text;

namespace MarketERP.Controllers;

public class EmployeeShiftsController : Controller
{
    private readonly AppDbContext _context;

    private static readonly IReadOnlyList<EmployeeShiftTypeOption> ShiftTypes =
    [
        new()
        {
            Value = "Sabah",
            Label = "Sabah",
            DescriptionValue = "Sabah vardiyası",
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(16, 0, 0)
        },
        new()
        {
            Value = "Aksam",
            Label = "Akşam",
            DescriptionValue = "Akşam vardiyası",
            StartTime = new TimeSpan(16, 0, 0),
            EndTime = TimeSpan.Zero
        },
        new()
        {
            Value = "Gece",
            Label = "Gece",
            DescriptionValue = "Gece vardiyası",
            StartTime = TimeSpan.Zero,
            EndTime = new TimeSpan(8, 0, 0)
        },
        new()
        {
            Value = "TamGun",
            Label = "Tam gün",
            DescriptionValue = "Tam gün",
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(18, 0, 0)
        },
        new()
        {
            Value = "Izinli",
            Label = "İzinli",
            DescriptionValue = "İzinli",
            StartTime = TimeSpan.Zero,
            EndTime = TimeSpan.Zero,
            IsLeave = true
        },
        new()
        {
            Value = "Bos",
            Label = "Boş / atanmadı",
            DescriptionValue = string.Empty,
            IsEmpty = true
        }
    ];

    public EmployeeShiftsController(AppDbContext context)
    {
        _context = context;
    }

    [PermissionAuthorize("reports.employee", "employee.view", "leave.request.create", "stock.view")]
    public async Task<IActionResult> Index(DateTime? weekStart)
    {
        if (!CanViewShifts())
        {
            return Forbid();
        }

        var monday = GetMonday(weekStart?.Date ?? DateTime.Today);
        var weekEnd = monday.AddDays(6);
        var weekDays = Enumerable.Range(0, 7).Select(day => monday.AddDays(day)).ToList();
        var canManage = CanManageShifts();
        var employeeId = HttpContext.Session.GetInt32("EmployeeId");

        var shiftsQuery = _context.EmployeeShifts
            .AsNoTracking()
            .Include(s => s.Employee)
            .Where(s => s.ShiftDate.Date >= monday && s.ShiftDate.Date <= weekEnd);

        if (!canManage)
        {
            if (!employeeId.HasValue)
            {
                return Forbid();
            }

            shiftsQuery = shiftsQuery.Where(s => s.EmployeeId == employeeId.Value);
        }

        var shifts = await shiftsQuery
            .OrderBy(s => s.Employee!.FullName)
            .ThenBy(s => s.ShiftDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        List<Employee> employees;
        if (canManage)
        {
            employees = await _context.Employees
                .AsNoTracking()
                .Where(e => e.IsActive)
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }
        else
        {
            employees = await _context.Employees
                .AsNoTracking()
                .Where(e => e.Id == employeeId)
                .ToListAsync();
        }

        ViewBag.Employees = new SelectList(employees, "Id", "FullName");

        var rows = employees.Select(employee =>
        {
            var employeeShifts = shifts.Where(s => s.EmployeeId == employee.Id).ToList();
            return new EmployeeShiftScheduleRowViewModel
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                Position = employee.Position,
                ShiftsByDay = weekDays.ToDictionary(
                    day => day,
                    day => employeeShifts
                        .Where(s => s.ShiftDate.Date == day)
                        .Select(CreateDisplayItem)
                        .ToList()),
                WeeklyHours = employeeShifts.Sum(GetShiftHours)
            };
        }).ToList();

        var model = new EmployeeShiftScheduleViewModel
        {
            WeekStart = monday,
            WeekEnd = weekEnd,
            CanManageShifts = canManage,
            IsOwnShiftView = !canManage,
            WeekDays = weekDays,
            Rows = rows,
            ShiftTypes = ShiftTypes.ToList(),
            DailyEmployeeCounts = weekDays.ToDictionary(
                day => day,
                day => shifts
                    .Where(s => s.ShiftDate.Date == day && !IsLeave(s))
                    .Select(s => s.EmployeeId)
                    .Distinct()
                    .Count())
        };

        return View(model);
    }

    [HttpGet]
    [PermissionAuthorize("reports.employee", "employee.view")]
    public async Task<IActionResult> ExportWeeklyCsv(DateTime? weekStart)
    {
        if (!CanManageShifts())
        {
            return Forbid();
        }

        var exportData = await BuildWeeklyExportDataAsync(weekStart);
        var csv = new StringBuilder();
        AppendCsvRow(csv, GetExportHeaders());

        foreach (var row in exportData.Rows)
        {
            var values = new List<string>
            {
                row.EmployeeName,
                row.Position
            };
            values.AddRange(row.DayValues);
            values.Add(row.WeeklyHours.ToString("0.##", CultureInfo.InvariantCulture));
            AppendCsvRow(csv, values);
        }

        var fileBytes = AddUtf8Bom(Encoding.UTF8.GetBytes(csv.ToString()));
        return File(
            fileBytes,
            "text/csv; charset=utf-8",
            $"haftalik-vardiya-{exportData.WeekStart:yyyy-MM-dd}.csv");
    }

    [HttpGet]
    [PermissionAuthorize("reports.employee", "employee.view")]
    public async Task<IActionResult> ExportWeeklyExcel(DateTime? weekStart)
    {
        if (!CanManageShifts())
        {
            return Forbid();
        }

        var exportData = await BuildWeeklyExportDataAsync(weekStart);
        var fileBytes = CreateExcelWorkbook(exportData);

        return File(
            fileBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"haftalik-vardiya-{exportData.WeekStart:yyyy-MM-dd}.xlsx");
    }

    [HttpGet]
    [PermissionAuthorize("reports.employee", "employee.view")]
    public async Task<IActionResult> ExportWeeklyWord(DateTime? weekStart)
    {
        if (!CanManageShifts())
        {
            return Forbid();
        }

        var exportData = await BuildWeeklyExportDataAsync(weekStart);
        var html = new StringBuilder();
        html.Append(
            """
            <!DOCTYPE html>
            <html lang="tr">
            <head>
                <meta charset="utf-8">
                <title>Haftalık Vardiya Planı</title>
                <style>
                    body { font-family: Arial, sans-serif; color: #1f2937; }
                    h1 { color: #1d4ed8; margin-bottom: 4px; }
                    .date-range { margin-bottom: 18px; color: #4b5563; }
                    table { width: 100%; border-collapse: collapse; font-size: 10pt; }
                    th { background: #1d4ed8; color: white; font-weight: bold; }
                    th, td { border: 1px solid #94a3b8; padding: 6px; vertical-align: top; }
                    .today { background: #fff3cd; color: #1f2937; }
                </style>
            </head>
            <body>
            """);
        html.Append("<h1>Haftalık Vardiya Planı</h1>");
        html.Append(CultureInfo.InvariantCulture,
            $"<div class=\"date-range\">{exportData.WeekStart:dd.MM.yyyy} - {exportData.WeekEnd:dd.MM.yyyy}</div>");
        html.Append("<table><thead><tr>");

        var headers = GetExportHeaders();
        for (var index = 0; index < headers.Count; index++)
        {
            html.Append(CultureInfo.InvariantCulture,
                $"<th class=\"{GetTodayCssClass(exportData.WeekStart, index)}\">{WebUtility.HtmlEncode(headers[index])}</th>");
        }

        html.Append("</tr></thead><tbody>");
        foreach (var row in exportData.Rows)
        {
            var values = new List<string>
            {
                row.EmployeeName,
                row.Position
            };
            values.AddRange(row.DayValues);
            values.Add(row.WeeklyHours.ToString("0.##", CultureInfo.InvariantCulture));

            html.Append("<tr>");
            for (var index = 0; index < values.Count; index++)
            {
                html.Append(CultureInfo.InvariantCulture,
                    $"<td class=\"{GetTodayCssClass(exportData.WeekStart, index)}\">{WebUtility.HtmlEncode(values[index])}</td>");
            }
            html.Append("</tr>");
        }

        html.Append("</tbody></table></body></html>");
        var fileBytes = AddUtf8Bom(Encoding.UTF8.GetBytes(html.ToString()));
        return File(
            fileBytes,
            "application/msword; charset=utf-8",
            $"haftalik-vardiya-{exportData.WeekStart:yyyy-MM-dd}.doc");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("reports.employee", "employee.view")]
    public async Task<IActionResult> Add(
        int employeeId,
        DateTime shiftDate,
        string shiftType,
        string? note,
        DateTime? weekStart)
    {
        if (!CanManageShifts())
        {
            return Forbid();
        }

        var employeeExists = await _context.Employees
            .AnyAsync(e => e.Id == employeeId && e.IsActive);
        if (!employeeExists)
        {
            TempData["Error"] = "Aktif bir çalışan seçmelisiniz.";
            return RedirectToWeek(weekStart ?? shiftDate);
        }

        var option = FindShiftType(shiftType);
        if (option is null)
        {
            TempData["Error"] = "Geçerli bir şift tipi seçmelisiniz.";
            return RedirectToWeek(weekStart ?? shiftDate);
        }

        if (option.IsEmpty)
        {
            TempData["Info"] = "Boş / atanmadı seçimi yeni vardiya oluşturmaz. Mevcut kaydı kaldırmak için Sil düğmesini kullanın.";
            return RedirectToWeek(weekStart ?? shiftDate);
        }

        var normalizedDate = shiftDate.Date;
        var shift = await _context.EmployeeShifts
            .OrderBy(s => s.Id)
            .FirstOrDefaultAsync(s => s.EmployeeId == employeeId && s.ShiftDate.Date == normalizedDate);

        var isNew = shift is null;
        shift ??= new EmployeeShift
        {
            EmployeeId = employeeId,
            ShiftDate = normalizedDate
        };

        ApplyShiftType(shift, option, note);

        if (isNew)
        {
            _context.EmployeeShifts.Add(shift);
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = isNew
            ? "Vardiya başarıyla kaydedildi."
            : "Bu çalışanın seçili gündeki vardiyası güncellendi.";

        return RedirectToWeek(weekStart ?? normalizedDate);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("reports.employee", "employee.view")]
    public async Task<IActionResult> Edit(
        int id,
        int employeeId,
        DateTime shiftDate,
        string shiftType,
        string? note,
        DateTime? weekStart)
    {
        if (!CanManageShifts())
        {
            return Forbid();
        }

        var shift = await _context.EmployeeShifts.FindAsync(id);
        if (shift is null)
        {
            return NotFound();
        }

        var employeeExists = await _context.Employees
            .AnyAsync(e => e.Id == employeeId && e.IsActive);
        if (!employeeExists)
        {
            TempData["Error"] = "Aktif bir çalışan seçmelisiniz.";
            return RedirectToWeek(weekStart ?? shift.ShiftDate);
        }

        var option = FindShiftType(shiftType);
        if (option is null)
        {
            TempData["Error"] = "Geçerli bir şift tipi seçmelisiniz.";
            return RedirectToWeek(weekStart ?? shift.ShiftDate);
        }

        if (option.IsEmpty)
        {
            TempData["Info"] = "Boş / atanmadı seçimi mevcut kaydı otomatik silmez. Kaydı kaldırmak için Sil düğmesini kullanın.";
            return RedirectToWeek(weekStart ?? shift.ShiftDate);
        }

        var normalizedDate = shiftDate.Date;
        var duplicateExists = await _context.EmployeeShifts.AnyAsync(s =>
            s.Id != id &&
            s.EmployeeId == employeeId &&
            s.ShiftDate.Date == normalizedDate);

        if (duplicateExists)
        {
            TempData["Error"] = "Bu çalışan için seçilen tarihte zaten bir vardiya var.";
            return RedirectToWeek(weekStart ?? normalizedDate);
        }

        shift.EmployeeId = employeeId;
        shift.ShiftDate = normalizedDate;
        ApplyShiftType(shift, option, note);

        await _context.SaveChangesAsync();
        TempData["Success"] = "Vardiya güncellendi.";

        return RedirectToWeek(weekStart ?? normalizedDate);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("reports.employee", "employee.view")]
    public async Task<IActionResult> Delete(int id, DateTime? weekStart)
    {
        if (!CanManageShifts())
        {
            return Forbid();
        }

        var shift = await _context.EmployeeShifts.FindAsync(id);
        if (shift is null)
        {
            return NotFound();
        }

        _context.EmployeeShifts.Remove(shift);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Vardiya silindi.";

        return RedirectToWeek(weekStart ?? shift.ShiftDate);
    }

    private bool CanViewShifts()
    {
        return HasRole("Admin")
            || HasRole("Mağaza Müdürü")
            || HasRole("Kasiyer")
            || HasRole("Depo Sorumlusu")
            || HasRole("Toptan Satış Sorumlusu")
            || HttpContext.HasPermission("employee.view")
            || HttpContext.HasPermission("reports.employee");
    }

    private bool CanManageShifts()
    {
        return HasRole("Admin") || HasRole("Mağaza Müdürü");
    }

    private bool HasRole(string roleName)
    {
        var roles = (HttpContext.Session.GetString("Roles") ?? string.Empty)
            .Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return roles.Contains(roleName, StringComparer.OrdinalIgnoreCase)
            || string.Equals(
                HttpContext.Session.GetString("Role"),
                roleName,
                StringComparison.OrdinalIgnoreCase);
    }

    private IActionResult RedirectToWeek(DateTime date)
    {
        return RedirectToAction(nameof(Index), new
        {
            weekStart = GetMonday(date.Date).ToString("yyyy-MM-dd")
        });
    }

    private static DateTime GetMonday(DateTime date)
    {
        var difference = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-difference).Date;
    }

    private static EmployeeShiftTypeOption? FindShiftType(string? value)
    {
        return ShiftTypes.FirstOrDefault(type =>
            type.Value.Equals(value?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static void ApplyShiftType(
        EmployeeShift shift,
        EmployeeShiftTypeOption option,
        string? note)
    {
        shift.StartTime = option.StartTime ?? TimeSpan.Zero;
        shift.EndTime = option.EndTime ?? TimeSpan.Zero;

        var cleanNote = note?.Trim();
        shift.Description = string.IsNullOrWhiteSpace(cleanNote)
            ? option.DescriptionValue
            : $"{option.DescriptionValue} | {cleanNote}";
    }

    private static EmployeeShiftDisplayItem CreateDisplayItem(EmployeeShift shift)
    {
        var option = GetShiftType(shift);
        var descriptionParts = (shift.Description ?? string.Empty)
            .Split('|', 2, StringSplitOptions.TrimEntries);

        var note = descriptionParts.Length > 1
            ? descriptionParts[1]
            : option is null && !string.IsNullOrWhiteSpace(shift.Description)
                ? shift.Description
                : string.Empty;

        return new EmployeeShiftDisplayItem
        {
            Id = shift.Id,
            EmployeeId = shift.EmployeeId,
            ShiftDate = shift.ShiftDate.Date,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            ShiftTypeValue = option?.Value ?? string.Empty,
            ShiftLabel = option?.Label ?? "Özel vardiya",
            Note = note,
            IsLeave = option?.IsLeave == true
        };
    }

    private static EmployeeShiftTypeOption? GetShiftType(EmployeeShift shift)
    {
        var descriptionType = (shift.Description ?? string.Empty)
            .Split('|', 2, StringSplitOptions.TrimEntries)[0];

        var byDescription = ShiftTypes.FirstOrDefault(type =>
            !type.IsEmpty &&
            type.DescriptionValue.Equals(descriptionType, StringComparison.OrdinalIgnoreCase));
        if (byDescription is not null)
        {
            return byDescription;
        }

        if (descriptionType.Contains("izin", StringComparison.OrdinalIgnoreCase))
        {
            return FindShiftType("Izinli");
        }

        return ShiftTypes.FirstOrDefault(type =>
            !type.IsEmpty &&
            !type.IsLeave &&
            type.StartTime == shift.StartTime &&
            type.EndTime == shift.EndTime);
    }

    private static bool IsLeave(EmployeeShift shift)
    {
        return GetShiftType(shift)?.IsLeave == true;
    }

    private static decimal GetShiftHours(EmployeeShift shift)
    {
        if (IsLeave(shift))
        {
            return 0;
        }

        var duration = shift.EndTime - shift.StartTime;
        if (duration <= TimeSpan.Zero)
        {
            duration = duration.Add(TimeSpan.FromDays(1));
        }

        return (decimal)duration.TotalHours;
    }

    private static string GetCsvShiftText(EmployeeShift shift)
    {
        var shiftType = GetShiftType(shift);
        return shiftType?.Value switch
        {
            "Sabah" => "Sabah 08:00-16:00",
            "Aksam" => "Akşam 16:00-00:00",
            "Gece" => "Gece 00:00-08:00",
            "TamGun" => "Tam gün 09:00-18:00",
            "Izinli" => "İzinli",
            _ => $"Özel {shift.StartTime:hh\\:mm}-{shift.EndTime:hh\\:mm}"
        };
    }

    private async Task<WeeklyShiftExportData> BuildWeeklyExportDataAsync(DateTime? weekStart)
    {
        var monday = GetMonday(weekStart?.Date ?? DateTime.Today);
        var weekEndExclusive = monday.AddDays(7);
        var weekDays = Enumerable.Range(0, 7)
            .Select(day => monday.AddDays(day))
            .ToList();

        var employees = await _context.Employees
            .AsNoTracking()
            .Where(employee => employee.IsActive)
            .OrderBy(employee => employee.FullName)
            .ToListAsync();

        var shifts = await _context.EmployeeShifts
            .AsNoTracking()
            .Where(shift =>
                shift.ShiftDate >= monday &&
                shift.ShiftDate < weekEndExclusive)
            .OrderBy(shift => shift.EmployeeId)
            .ThenBy(shift => shift.ShiftDate)
            .ThenBy(shift => shift.StartTime)
            .ToListAsync();

        var rows = employees.Select(employee =>
        {
            var employeeShifts = shifts
                .Where(shift => shift.EmployeeId == employee.Id)
                .ToList();

            var dayValues = weekDays.Select(day =>
            {
                var dayShifts = employeeShifts
                    .Where(shift => shift.ShiftDate.Date == day)
                    .Select(GetCsvShiftText)
                    .ToList();

                return dayShifts.Count > 0
                    ? string.Join(" / ", dayShifts)
                    : "-";
            }).ToList();

            return new WeeklyShiftExportRow(
                employee.FullName,
                employee.Position ?? string.Empty,
                dayValues,
                employeeShifts.Sum(GetShiftHours));
        }).ToList();

        return new WeeklyShiftExportData(
            monday,
            monday.AddDays(6),
            rows);
    }

    private static IReadOnlyList<string> GetExportHeaders()
    {
        return
        [
            "Çalışan",
            "Pozisyon",
            "Pazartesi",
            "Salı",
            "Çarşamba",
            "Perşembe",
            "Cuma",
            "Cumartesi",
            "Pazar",
            "Haftalık Toplam Saat"
        ];
    }

    private static byte[] CreateExcelWorkbook(WeeklyShiftExportData exportData)
    {
        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, true))
        {
            AddZipEntry(
                archive,
                "[Content_Types].xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
                </Types>
                """);

            AddZipEntry(
                archive,
                "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);

            AddZipEntry(
                archive,
                "xl/workbook.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets>
                    <sheet name="Haftalık Vardiya Planı" sheetId="1" r:id="rId1"/>
                  </sheets>
                </workbook>
                """);

            AddZipEntry(
                archive,
                "xl/_rels/workbook.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
                </Relationships>
                """);

            AddZipEntry(archive, "xl/styles.xml", CreateExcelStylesXml());
            AddZipEntry(archive, "xl/worksheets/sheet1.xml", CreateExcelSheetXml(exportData));
        }

        return output.ToArray();
    }

    private static string CreateExcelSheetXml(WeeklyShiftExportData exportData)
    {
        var headers = GetExportHeaders();
        var sheet = new StringBuilder();
        var lastRow = exportData.Rows.Count + 4;
        var columnsXml = CreateExcelColumnsXml(exportData);
        var todayOffset = DateTime.Today >= exportData.WeekStart
            && DateTime.Today <= exportData.WeekEnd
                ? (DateTime.Today - exportData.WeekStart).Days
                : -1;

        sheet.Append(
            $"""
             <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
             <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
               <dimension ref="A1:J{lastRow}"/>
               <sheetViews>
                 <sheetView workbookViewId="0">
                   <pane ySplit="4" topLeftCell="A5" activePane="bottomLeft" state="frozen"/>
                 </sheetView>
               </sheetViews>
               {columnsXml}
               <sheetData>
             """);

        sheet.Append("<row r=\"1\" ht=\"26\" customHeight=\"1\">");
        sheet.Append(CreateInlineExcelCell("A1", "Haftalık Vardiya Planı", 1));
        sheet.Append("</row>");

        sheet.Append("<row r=\"2\">");
        sheet.Append(CreateInlineExcelCell(
            "A2",
            $"{exportData.WeekStart:dd.MM.yyyy} - {exportData.WeekEnd:dd.MM.yyyy}",
            2));
        sheet.Append("</row>");

        sheet.Append("<row r=\"4\" ht=\"30\" customHeight=\"1\">");
        for (var columnIndex = 0; columnIndex < headers.Count; columnIndex++)
        {
            var styleIndex = columnIndex >= 2
                && columnIndex <= 8
                && columnIndex - 2 == todayOffset
                    ? 6
                    : 3;

            sheet.Append(CreateInlineExcelCell(
                $"{GetExcelColumnName(columnIndex + 1)}4",
                headers[columnIndex],
                styleIndex));
        }
        sheet.Append("</row>");

        for (var rowIndex = 0; rowIndex < exportData.Rows.Count; rowIndex++)
        {
            var excelRow = rowIndex + 5;
            var row = exportData.Rows[rowIndex];
            var values = new List<string>
            {
                row.EmployeeName,
                row.Position
            };
            values.AddRange(row.DayValues);

            sheet.Append(CultureInfo.InvariantCulture, $"<row r=\"{excelRow}\">");
            for (var columnIndex = 0; columnIndex < values.Count; columnIndex++)
            {
                var styleIndex = columnIndex >= 2
                    && columnIndex <= 8
                    && columnIndex - 2 == todayOffset
                        ? 5
                        : 4;

                sheet.Append(CreateInlineExcelCell(
                    $"{GetExcelColumnName(columnIndex + 1)}{excelRow}",
                    values[columnIndex],
                    styleIndex));
            }

            sheet.Append(CultureInfo.InvariantCulture,
                $"<c r=\"J{excelRow}\" s=\"4\"><v>{row.WeeklyHours.ToString("0.##", CultureInfo.InvariantCulture)}</v></c>");
            sheet.Append("</row>");
        }

        sheet.Append(
            """
              </sheetData>
              <mergeCells count="2">
                <mergeCell ref="A1:J1"/>
                <mergeCell ref="A2:J2"/>
              </mergeCells>
              <pageMargins left="0.3" right="0.3" top="0.5" bottom="0.5" header="0.2" footer="0.2"/>
              <pageSetup orientation="landscape" fitToWidth="1" fitToHeight="0"/>
            </worksheet>
            """);

        return sheet.ToString();
    }

    private static string CreateExcelColumnsXml(WeeklyShiftExportData exportData)
    {
        var headers = GetExportHeaders();
        var columnValues = Enumerable.Range(0, headers.Count)
            .Select(index => new List<string> { headers[index] })
            .ToList();

        foreach (var row in exportData.Rows)
        {
            columnValues[0].Add(row.EmployeeName);
            columnValues[1].Add(row.Position);
            for (var dayIndex = 0; dayIndex < row.DayValues.Count; dayIndex++)
            {
                columnValues[dayIndex + 2].Add(row.DayValues[dayIndex]);
            }
            columnValues[9].Add(row.WeeklyHours.ToString("0.##", CultureInfo.InvariantCulture));
        }

        var columns = new StringBuilder("<cols>");
        for (var index = 0; index < columnValues.Count; index++)
        {
            var minimumWidth = index switch
            {
                0 => 20,
                1 => 18,
                >= 2 and <= 8 => 18,
                _ => 16
            };
            var width = Math.Clamp(
                columnValues[index].Max(value => value.Length) + 3,
                minimumWidth,
                38);

            columns.Append(CultureInfo.InvariantCulture,
                $"<col min=\"{index + 1}\" max=\"{index + 1}\" width=\"{width}\" customWidth=\"1\"/>");
        }
        columns.Append("</cols>");
        return columns.ToString();
    }

    private static string CreateExcelStylesXml()
    {
        return
            """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
              <fonts count="4">
                <font><sz val="11"/><name val="Calibri"/></font>
                <font><b/><sz val="16"/><color rgb="FF1D4ED8"/><name val="Calibri"/></font>
                <font><b/><sz val="11"/><color rgb="FFFFFFFF"/><name val="Calibri"/></font>
                <font><b/><sz val="11"/><color rgb="FF1F2937"/><name val="Calibri"/></font>
              </fonts>
              <fills count="4">
                <fill><patternFill patternType="none"/></fill>
                <fill><patternFill patternType="gray125"/></fill>
                <fill><patternFill patternType="solid"><fgColor rgb="FF1D4ED8"/><bgColor indexed="64"/></patternFill></fill>
                <fill><patternFill patternType="solid"><fgColor rgb="FFFFF3CD"/><bgColor indexed="64"/></patternFill></fill>
              </fills>
              <borders count="2">
                <border><left/><right/><top/><bottom/><diagonal/></border>
                <border>
                  <left style="thin"><color rgb="FF94A3B8"/></left>
                  <right style="thin"><color rgb="FF94A3B8"/></right>
                  <top style="thin"><color rgb="FF94A3B8"/></top>
                  <bottom style="thin"><color rgb="FF94A3B8"/></bottom>
                  <diagonal/>
                </border>
              </borders>
              <cellStyleXfs count="1">
                <xf numFmtId="0" fontId="0" fillId="0" borderId="0"/>
              </cellStyleXfs>
              <cellXfs count="7">
                <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/>
                <xf numFmtId="0" fontId="1" fillId="0" borderId="0" xfId="0"/>
                <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/>
                <xf numFmtId="0" fontId="2" fillId="2" borderId="1" xfId="0" applyAlignment="1"><alignment horizontal="center" vertical="center" wrapText="1"/></xf>
                <xf numFmtId="0" fontId="0" fillId="0" borderId="1" xfId="0" applyAlignment="1"><alignment vertical="top" wrapText="1"/></xf>
                <xf numFmtId="0" fontId="0" fillId="3" borderId="1" xfId="0" applyAlignment="1"><alignment vertical="top" wrapText="1"/></xf>
                <xf numFmtId="0" fontId="3" fillId="3" borderId="1" xfId="0" applyAlignment="1"><alignment horizontal="center" vertical="center" wrapText="1"/></xf>
              </cellXfs>
              <cellStyles count="1">
                <cellStyle name="Normal" xfId="0" builtinId="0"/>
              </cellStyles>
              <dxfs count="0"/>
              <tableStyles count="0" defaultTableStyle="TableStyleMedium2" defaultPivotStyle="PivotStyleLight16"/>
            </styleSheet>
            """;
    }

    private static string CreateInlineExcelCell(
        string reference,
        string value,
        int styleIndex)
    {
        var escapedValue = SecurityElement.Escape(value) ?? string.Empty;
        return $"<c r=\"{reference}\" t=\"inlineStr\" s=\"{styleIndex}\"><is><t xml:space=\"preserve\">{escapedValue}</t></is></c>";
    }

    private static string GetExcelColumnName(int columnNumber)
    {
        var columnName = string.Empty;
        while (columnNumber > 0)
        {
            columnNumber--;
            columnName = (char)('A' + columnNumber % 26) + columnName;
            columnNumber /= 26;
        }

        return columnName;
    }

    private static void AddZipEntry(
        ZipArchive archive,
        string path,
        string content)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Fastest);
        using var writer = new StreamWriter(
            entry.Open(),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(content);
    }

    private static string GetTodayCssClass(DateTime weekStart, int columnIndex)
    {
        return columnIndex >= 2
            && columnIndex <= 8
            && weekStart.AddDays(columnIndex - 2) == DateTime.Today
                ? "today"
                : string.Empty;
    }

    private static byte[] AddUtf8Bom(byte[] content)
    {
        var bom = Encoding.UTF8.GetPreamble();
        var fileBytes = new byte[bom.Length + content.Length];
        Buffer.BlockCopy(bom, 0, fileBytes, 0, bom.Length);
        Buffer.BlockCopy(content, 0, fileBytes, bom.Length, content.Length);
        return fileBytes;
    }

    private static void AppendCsvRow(StringBuilder csv, IEnumerable<string> values)
    {
        csv.AppendLine(string.Join(",", values.Select(EscapeCsvValue)));
    }

    private static string EscapeCsvValue(string? value)
    {
        return $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
    }

    private sealed record WeeklyShiftExportData(
        DateTime WeekStart,
        DateTime WeekEnd,
        List<WeeklyShiftExportRow> Rows);

    private sealed record WeeklyShiftExportRow(
        string EmployeeName,
        string Position,
        List<string> DayValues,
        decimal WeeklyHours);
}
