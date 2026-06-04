using MarketERP.Data;
using MarketERP.Helpers;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;

namespace MarketERP.Controllers
{
    public class CashRegisterController : Controller
    {
        private readonly AppDbContext _context;

        public CashRegisterController(AppDbContext context)
        {
            _context = context;
        }

        [PermissionAuthorize("cash.closing.create")]
        public IActionResult MyClosings()
        {
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (employeeId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var closings = _context.CashRegisterClosings
                .Where(c => c.EmployeeId == employeeId.Value)
                .OrderByDescending(c => c.ClosingDate)
                .ToList();

            return View(closings);
        }

        [PermissionAuthorize("cash.closing.create")]
        public IActionResult Create()
        {
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (employeeId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var alreadyClosed = _context.CashRegisterClosings.Any(c =>
                c.EmployeeId == employeeId.Value &&
                c.ClosingDate >= today &&
                c.ClosingDate < tomorrow);

            if (alreadyClosed)
            {
                TempData["Error"] = "Bugün için kasa kapanışı zaten oluşturulmuş.";
                return RedirectToAction("MyClosings");
            }

            var todaySales = _context.Sales
                .Where(s =>
                    s.EmployeeId == employeeId.Value &&
                    s.SaleDate >= today &&
                    s.SaleDate < tomorrow);

            var cashTotal = todaySales
                .Where(s => s.PaymentType == "Nakit")
                .Select(s => (decimal?)s.TotalAmount)
                .Sum() ?? 0;

            var cardTotal = todaySales
                .Where(s => s.PaymentType == "Kart")
                .Select(s => (decimal?)s.TotalAmount)
                .Sum() ?? 0;

            var totalAmount = todaySales
                .Select(s => (decimal?)s.TotalAmount)
                .Sum() ?? 0;

            ViewBag.CashTotal = cashTotal;
            ViewBag.CardTotal = cardTotal;
            ViewBag.TotalAmount = totalAmount;

            return View();
        }

        [HttpPost]
        [PermissionAuthorize("cash.closing.create")]
        public IActionResult Create(decimal declaredCashAmount, string? note)
        {
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (employeeId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var alreadyClosed = _context.CashRegisterClosings.Any(c =>
                c.EmployeeId == employeeId.Value &&
                c.ClosingDate >= today &&
                c.ClosingDate < tomorrow);

            if (alreadyClosed)
            {
                TempData["Error"] = "Bugün için kasa kapanışı zaten oluşturulmuş.";
                return RedirectToAction("MyClosings");
            }

            var todaySales = _context.Sales
                .Where(s =>
                    s.EmployeeId == employeeId.Value &&
                    s.SaleDate >= today &&
                    s.SaleDate < tomorrow);

            var cashTotal = todaySales
                .Where(s => s.PaymentType == "Nakit")
                .Select(s => (decimal?)s.TotalAmount)
                .Sum() ?? 0;

            var cardTotal = todaySales
                .Where(s => s.PaymentType == "Kart")
                .Select(s => (decimal?)s.TotalAmount)
                .Sum() ?? 0;

            var totalAmount = todaySales
                .Select(s => (decimal?)s.TotalAmount)
                .Sum() ?? 0;

            var closing = new CashRegisterClosing
            {
                EmployeeId = employeeId.Value,
                ClosingDate = DateTime.Now,
                CashSalesTotal = cashTotal,
                CardSalesTotal = cardTotal,
                TotalSalesAmount = totalAmount,
                DeclaredCashAmount = declaredCashAmount,
                CashDifference = declaredCashAmount - cashTotal,
                Note = note,
                Status = "Beklemede",
                CreatedAt = DateTime.Now
            };

            _context.CashRegisterClosings.Add(closing);
            _context.SaveChanges();

            TempData["Success"] = "Kasa kapanışı oluşturuldu. Onay için mağaza müdürüne/admin'e gönderildi.";

            return RedirectToAction("MyClosings");
        }
    }
}