using MarketERP.Data;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarketERP.Helpers;

namespace MarketERP.Controllers
{
    [PermissionAuthorize("reports.employee")]
    public class EmployeeLeavesController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeeLeavesController(AppDbContext context)
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

            var leaves = _context.EmployeeLeaves
                .Include(l => l.Employee)
                .OrderByDescending(l => l.Id)
                .ToList();

            return View(leaves);
        }

        [HttpPost]
        public IActionResult Add(EmployeeLeave leave)
        {
            if (string.IsNullOrWhiteSpace(leave.Status))
            {
                leave.Status = "Beklemede";
            }

            _context.EmployeeLeaves.Add(leave);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Approve(int id)
        {
            var leave = _context.EmployeeLeaves.Find(id);

            if (leave != null)
            {
                leave.Status = "Onaylandı";
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Reject(int id)
        {
            var leave = _context.EmployeeLeaves.Find(id);

            if (leave != null)
            {
                leave.Status = "Reddedildi";
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var leave = _context.EmployeeLeaves.Find(id);

            if (leave != null)
            {
                _context.EmployeeLeaves.Remove(leave);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}