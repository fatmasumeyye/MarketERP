using MarketERP.Data;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using MarketERP.Helpers;

namespace MarketERP.Controllers
{
    [PermissionAuthorize("finance.view")]
    public class ExpensesController : Controller
    {
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.TotalIncome = _context.Sales.Sum(s => s.TotalAmount);
            ViewBag.TotalExpense = _context.Expenses.Sum(e => e.Amount);
            ViewBag.NetProfit = ViewBag.TotalIncome - ViewBag.TotalExpense;

            var expenses = _context.Expenses
                .OrderByDescending(e => e.ExpenseDate)
                .ToList();

            return View(expenses);
        }

        [HttpPost]
        public IActionResult Add(Expense expense)
        {
            _context.Expenses.Add(expense);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var expense = _context.Expenses.Find(id);

            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}