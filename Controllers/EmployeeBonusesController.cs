using MarketERP.Data;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarketERP.Helpers;

namespace MarketERP.Controllers
{
    [PermissionAuthorize("reports.employee")]
    public class EmployeeBonusesController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeeBonusesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Employees = new SelectList(
                _context.Employees.Where(e => e.IsActive).OrderBy(e => e.FullName).ToList(),
                "Id",
                "FullName");

            var bonuses = _context.EmployeeBonuses
                .Include(b => b.Employee)
                .OrderByDescending(b => b.BonusDate)
                .ToList();

            return View(bonuses);
        }

        [HttpPost]
        public IActionResult Add(EmployeeBonus bonus)
        {
            if (!_context.Employees.Any(e => e.Id == bonus.EmployeeId && e.IsActive))
            {
                TempData["Error"] = "Pasif veya bulunamayan çalışana prim atanamaz.";
                return RedirectToAction("Index");
            }

            _context.EmployeeBonuses.Add(bonus);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var bonus = _context.EmployeeBonuses.Find(id);

            if (bonus != null)
            {
                _context.EmployeeBonuses.Remove(bonus);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
