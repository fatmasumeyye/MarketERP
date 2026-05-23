using MarketERP.Data;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarketERP.Helpers;

namespace MarketERP.Controllers
{
    [PermissionAuthorize("reports.employee")]
    public class EmployeeShiftsController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeeShiftsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Employees = new SelectList(_context.Employees.ToList(), "Id", "FullName");

            var shifts = _context.EmployeeShifts
                .Include(s => s.Employee)
                .OrderByDescending(s => s.ShiftDate)
                .ToList();

            return View(shifts);
        }

        [HttpPost]
        public IActionResult Add(EmployeeShift shift)
        {
            _context.EmployeeShifts.Add(shift);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var shift = _context.EmployeeShifts.Find(id);

            if (shift != null)
            {
                _context.EmployeeShifts.Remove(shift);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}