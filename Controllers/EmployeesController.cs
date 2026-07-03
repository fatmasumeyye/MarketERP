using MarketERP.Data;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using MarketERP.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MarketERP.Controllers
{
    [PermissionAuthorize("employee.view")]
    public class EmployeesController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string status = "active")
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            status = NormalizeStatusFilter(status);
            var query = _context.Employees.AsQueryable();
            if (status == "active") query = query.Where(e => e.IsActive);
            else if (status == "inactive") query = query.Where(e => !e.IsActive);

            ViewBag.Status = status;
            var employees = query.OrderBy(e => e.FullName).ToList();
            return View(employees);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("employee.manage")]
        public IActionResult Add(Employee employee)
        {
            employee.Salary = null;
            employee.IsActive = true;
            _context.Employees.Add(employee);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("employee.manage")]
        public IActionResult Delete(int id)
        {
            var employee = _context.Employees.Find(id);

            if (employee == null)
            {
                TempData["Error"] = "Çalışan bulunamadı.";
                return RedirectToAction("Index");
            }

            if (HttpContext.Session.GetInt32("EmployeeId") == employee.Id)
            {
                TempData["Error"] = "Aktif oturumunuza ait çalışan kaydını pasif yapamazsınız.";
                return RedirectToAction("Index");
            }

            if (!employee.IsActive)
            {
                TempData["Info"] = "Çalışan zaten pasif durumda.";
                return RedirectToAction("Index", new { status = "inactive" });
            }

            employee.IsActive = false;
            _context.SaveChanges();
            TempData["Success"] = "Çalışan pasif duruma alındı. Geçmiş işlemleri korunuyor.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("employee.manage")]
        public IActionResult Activate(int id)
        {
            var employee = _context.Employees.Find(id);
            if (employee == null)
            {
                TempData["Error"] = "Çalışan bulunamadı.";
                return RedirectToAction("Index", new { status = "inactive" });
            }

            if (employee.IsActive)
            {
                TempData["Info"] = "Çalışan zaten aktif durumda.";
                return RedirectToAction("Index");
            }

            employee.IsActive = true;
            _context.SaveChanges();
            TempData["Success"] = "Çalışan yeniden aktifleştirildi.";
            return RedirectToAction("Index", new { status = "inactive" });
        }

        public async Task<IActionResult> Details(int id)
        {
            var employee = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return NotFound();
            var leaves = await _context.EmployeeLeaves.AsNoTracking().Where(l => l.EmployeeId == id).ToListAsync();
            var shifts = await _context.EmployeeShifts.AsNoTracking().Where(s => s.EmployeeId == id).ToListAsync();

            return View(new EmployeeDetailViewModel
            {
                Employee = employee,
                TotalLeaveRequests = leaves.Count,
                ApprovedLeaveRequests = leaves.Count(l => l.Status == "Onaylandı"),
                ApprovedLeaveDays = leaves.Where(l => l.Status == "Onaylandı").Sum(l => Math.Max(1, (l.EndDate.Date - l.StartDate.Date).Days + 1)),
                TotalShifts = shifts.Count,
                TotalShiftHours = shifts.Where(s => !s.Description.StartsWith("İzinli", StringComparison.OrdinalIgnoreCase)).Sum(CalculateShiftHours),
                NextShift = shifts.Where(s => s.ShiftDate.Date >= DateTime.Today).OrderBy(s => s.ShiftDate).FirstOrDefault()
            });
        }

        [PermissionAuthorize("employee.manage")]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            return employee == null ? NotFound() : View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("employee.manage")]
        public async Task<IActionResult> Edit(Employee model)
        {
            var employee = await _context.Employees.FindAsync(model.Id);
            if (employee == null) return NotFound();
            if (string.IsNullOrWhiteSpace(model.FullName)) ModelState.AddModelError(nameof(model.FullName), "Çalışan adı zorunludur.");
            if (!ModelState.IsValid) return View(model);

            employee.FullName = model.FullName.Trim();
            employee.Phone = model.Phone?.Trim() ?? string.Empty;
            employee.Email = model.Email?.Trim();
            employee.Position = model.Position?.Trim() ?? string.Empty;
            employee.HireDate = model.HireDate;
            employee.Salary = model.Salary;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Çalışan bilgileri güncellendi.";
            return RedirectToAction(nameof(Details), new { id = employee.Id });
        }

        private static decimal CalculateShiftHours(EmployeeShift shift)
        {
            var duration = shift.EndTime - shift.StartTime;
            if (duration <= TimeSpan.Zero) duration = duration.Add(TimeSpan.FromDays(1));
            return (decimal)duration.TotalHours;
        }

        private static string NormalizeStatusFilter(string? status)
        {
            return status is "all" or "inactive" ? status : "active";
        }
    }
}
