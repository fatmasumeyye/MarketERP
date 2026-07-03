using MarketERP.Data;
using MarketERP.Helpers;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MarketERP.Controllers
{
    public class EmployeeLeavesController : Controller
    {
        private const string PendingStatus = "Beklemede";
        private const string ApprovedStatus = "Onaylandı";
        private const string RejectedStatus = "Reddedildi";

        private readonly AppDbContext _context;

        public EmployeeLeavesController(AppDbContext context)
        {
            _context = context;
        }

        [PermissionAuthorize("leave.request.view")]
        public async Task<IActionResult> Index()
        {
            var leaves = await _context.EmployeeLeaves
                .Include(l => l.Employee)
                .OrderByDescending(l => l.Id)
                .ToListAsync();

            var employeeLeaves = leaves
                .GroupBy(l => l.EmployeeId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var leave in leaves)
            {
                PopulateLeaveSummary(leave, employeeLeaves[leave.EmployeeId]);
            }

            ViewBag.Employees = new SelectList(
                await _context.Employees.Where(e => e.IsActive).OrderBy(e => e.FullName).ToListAsync(),
                "Id",
                "FullName");
            ViewBag.PendingCount = leaves.Count(l => l.Status == PendingStatus);
            ViewBag.ApprovedCount = leaves.Count(l => l.Status == ApprovedStatus);
            ViewBag.RejectedCount = leaves.Count(l => l.Status == RejectedStatus);
            ViewBag.TotalCount = leaves.Count;

            return View(leaves);
        }

        [PermissionAuthorize("leave.request.create")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var employeeId = HttpContext.Session.GetInt32("EmployeeId")!.Value;
            await PopulateEmployeeSummaryAsync(employeeId);
            return View();
        }

        [PermissionAuthorize("leave.request.create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            DateTime startDate,
            DateTime endDate,
            string leaveReason)
        {
            var employeeId = HttpContext.Session.GetInt32("EmployeeId")!.Value;
            var validationError = await ValidateLeaveAsync(
                employeeId,
                startDate,
                endDate,
                leaveReason,
                includePendingLeaves: true);

            if (validationError != null)
            {
                TempData["Error"] = validationError;
                await PopulateEmployeeSummaryAsync(employeeId);
                return View();
            }

            _context.EmployeeLeaves.Add(new EmployeeLeave
            {
                EmployeeId = employeeId,
                StartDate = startDate.Date,
                EndDate = endDate.Date,
                LeaveReason = leaveReason.Trim(),
                Status = PendingStatus
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "İzin talebiniz oluşturuldu.";
            return RedirectToAction(nameof(MyLeaves));
        }

        [PermissionAuthorize("leave.request.create")]
        [HttpGet]
        [ActionName("Request")]
        public IActionResult LegacyRequest()
        {
            return RedirectToAction(nameof(Create));
        }

        [PermissionAuthorize("leave.request.create")]
        [HttpGet]
        public async Task<IActionResult> MyLeaves()
        {
            var employeeId = HttpContext.Session.GetInt32("EmployeeId")!.Value;
            var leaves = await _context.EmployeeLeaves
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.Id)
                .ToListAsync();

            foreach (var leave in leaves)
            {
                PopulateLeaveSummary(leave, leaves);
            }

            await PopulateEmployeeSummaryAsync(employeeId, leaves);
            return View(leaves);
        }

        [PermissionAuthorize("leave.request.approve")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(EmployeeLeave leave)
        {
            var validationError = await ValidateLeaveAsync(
                leave.EmployeeId,
                leave.StartDate,
                leave.EndDate,
                leave.LeaveReason,
                includePendingLeaves: false);

            if (validationError != null)
            {
                TempData["Error"] = validationError;
                return RedirectToAction(nameof(Index));
            }

            leave.StartDate = leave.StartDate.Date;
            leave.EndDate = leave.EndDate.Date;
            leave.LeaveReason = leave.LeaveReason.Trim();
            leave.Status = PendingStatus;

            _context.EmployeeLeaves.Add(leave);
            await _context.SaveChangesAsync();

            TempData["Success"] = "İzin kaydı oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        [PermissionAuthorize("leave.request.approve")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var leave = await _context.EmployeeLeaves
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leave == null)
            {
                return NotFound();
            }

            if (leave.Status != PendingStatus)
            {
                TempData["Error"] = "Yalnızca bekleyen izin talepleri onaylanabilir.";
                return RedirectToAction(nameof(Index));
            }

            if (LeaveCalculator.IsAnnualLeave(leave.LeaveReason))
            {
                var approvedLeaves = await _context.EmployeeLeaves
                    .Where(l =>
                        l.EmployeeId == leave.EmployeeId &&
                        l.Id != leave.Id &&
                        l.Status == ApprovedStatus)
                    .ToListAsync();

                var annualRight = LeaveCalculator.CalculateAnnualLeaveRight(
                    leave.Employee?.HireDate,
                    DateTime.Today);
                var usedDays = LeaveCalculator.CalculateAnnualLeaveDays(approvedLeaves);
                var requestedDays = LeaveCalculator.CalculateRequestedDays(
                    leave.StartDate,
                    leave.EndDate);

                if (requestedDays > Math.Max(0, annualRight - usedDays))
                {
                    TempData["Error"] = "Bu talep çalışanın kalan yıllık izin hakkını aşıyor.";
                    return RedirectToAction(nameof(Index));
                }
            }

            leave.Status = ApprovedStatus;
            await _context.SaveChangesAsync();
            TempData["Success"] = "İzin talebi onaylandı.";
            return RedirectToAction(nameof(Index));
        }

        [PermissionAuthorize("leave.request.approve")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var leave = await _context.EmployeeLeaves.FindAsync(id);
            if (leave == null)
            {
                return NotFound();
            }

            if (leave.Status != PendingStatus)
            {
                TempData["Error"] = "Yalnızca bekleyen izin talepleri reddedilebilir.";
                return RedirectToAction(nameof(Index));
            }

            leave.Status = RejectedStatus;
            await _context.SaveChangesAsync();
            TempData["Success"] = "İzin talebi reddedildi.";
            return RedirectToAction(nameof(Index));
        }

        [PermissionAuthorize("leave.request.approve")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var leave = await _context.EmployeeLeaves.FindAsync(id);
            if (leave == null)
            {
                return NotFound();
            }

            _context.EmployeeLeaves.Remove(leave);
            await _context.SaveChangesAsync();
            TempData["Success"] = "İzin kaydı silindi.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> ValidateLeaveAsync(
            int employeeId,
            DateTime startDate,
            DateTime endDate,
            string? leaveReason,
            bool includePendingLeaves)
        {
            if (!await _context.Employees.AnyAsync(e => e.Id == employeeId && e.IsActive))
            {
                return "Çalışan bulunamadı.";
            }

            if (startDate == default || endDate == default)
            {
                return "Başlangıç ve bitiş tarihi zorunludur.";
            }

            if (endDate.Date < startDate.Date)
            {
                return "Bitiş tarihi başlangıç tarihinden önce olamaz.";
            }

            if (string.IsNullOrWhiteSpace(leaveReason))
            {
                return "İzin nedeni zorunludur.";
            }

            if (!LeaveCalculator.IsAnnualLeave(leaveReason))
            {
                return null;
            }

            var employee = await _context.Employees.FindAsync(employeeId);
            var leaves = await _context.EmployeeLeaves
                .Where(l =>
                    l.EmployeeId == employeeId &&
                    (l.Status == ApprovedStatus ||
                     (includePendingLeaves && l.Status == PendingStatus)))
                .ToListAsync();

            var annualRight = LeaveCalculator.CalculateAnnualLeaveRight(
                employee?.HireDate,
                DateTime.Today);
            var reservedDays = LeaveCalculator.CalculateAnnualLeaveDays(leaves);
            var requestedDays = LeaveCalculator.CalculateRequestedDays(startDate, endDate);

            if (requestedDays > Math.Max(0, annualRight - reservedDays))
            {
                return $"Yetersiz yıllık izin hakkı. Kullanılabilir hak: {Math.Max(0, annualRight - reservedDays)} gün.";
            }

            return null;
        }

        private async Task PopulateEmployeeSummaryAsync(
            int employeeId,
            IReadOnlyCollection<EmployeeLeave>? leaves = null)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            var employeeLeaves = leaves ?? await _context.EmployeeLeaves
                .Where(l => l.EmployeeId == employeeId)
                .ToListAsync();

            var seniorityYear = LeaveCalculator.CalculateSeniorityYears(
                employee?.HireDate,
                DateTime.Today);
            var annualLeaveRight = LeaveCalculator.CalculateAnnualLeaveRight(
                employee?.HireDate,
                DateTime.Today);
            var usedLeaveDays = LeaveCalculator.CalculateAnnualLeaveDays(
                employeeLeaves.Where(l => l.Status == ApprovedStatus));
            var pendingLeaveDays = LeaveCalculator.CalculateAnnualLeaveDays(
                employeeLeaves.Where(l => l.Status == PendingStatus));
            var remainingLeaveDays = Math.Max(0, annualLeaveRight - usedLeaveDays);

            ViewBag.SeniorityYear = seniorityYear;
            ViewBag.AnnualLeaveRight = annualLeaveRight;
            ViewBag.UsedLeaveDays = usedLeaveDays;
            ViewBag.PendingLeaveDays = pendingLeaveDays;
            ViewBag.RemainingLeaveDays = remainingLeaveDays;
            ViewBag.RemainingAfterPending = Math.Max(
                0,
                remainingLeaveDays - pendingLeaveDays);
        }

        private static void PopulateLeaveSummary(
            EmployeeLeave leave,
            IEnumerable<EmployeeLeave> employeeLeaves)
        {
            var leaves = employeeLeaves.ToList();
            leave.RequestedDays = LeaveCalculator.CalculateRequestedDays(
                leave.StartDate,
                leave.EndDate);
            leave.SeniorityYear = LeaveCalculator.CalculateSeniorityYears(
                leave.Employee?.HireDate,
                DateTime.Today);
            leave.AnnualLeaveRight = LeaveCalculator.CalculateAnnualLeaveRight(
                leave.Employee?.HireDate,
                DateTime.Today);
            leave.UsedLeaveDays = LeaveCalculator.CalculateAnnualLeaveDays(
                leaves.Where(l => l.Status == ApprovedStatus));
            leave.PendingLeaveDays = LeaveCalculator.CalculateAnnualLeaveDays(
                leaves.Where(l => l.Status == PendingStatus));
            leave.RemainingLeaveDays = Math.Max(
                0,
                leave.AnnualLeaveRight - leave.UsedLeaveDays);
        }
    }
}
