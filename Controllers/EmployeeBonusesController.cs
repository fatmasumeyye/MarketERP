using MarketERP.Data;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MarketERP.Controllers
{
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

            ViewBag.Employees = new SelectList(_context.Employees.ToList(), "Id", "FullName");

            var bonuses = _context.EmployeeBonuses
                .Include(b => b.Employee)
                .OrderByDescending(b => b.BonusDate)
                .ToList();

            return View(bonuses);
        }

        [HttpPost]
        public IActionResult Add(EmployeeBonus bonus)
        {
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