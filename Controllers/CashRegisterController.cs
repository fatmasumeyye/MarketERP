using MarketERP.Data;
using MarketERP.Helpers;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketERP.Controllers
{
    public class CashRegisterController : Controller
    {
        private readonly AppDbContext _context;

        public CashRegisterController(AppDbContext context)
        {
            _context = context;
        }

        [PermissionAuthorize("sale.view.all", "sale.view.branch", "role.manage")]
        public IActionResult Index(string status)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var closingsQuery = _context.CashRegisterClosings
                .Include(c => c.Employee)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                closingsQuery = closingsQuery.Where(c => c.Status == status);
            }

            var closings = closingsQuery
                .OrderByDescending(c => c.ClosingDate)
                .ToList();

            ViewBag.Status = status;

            ViewBag.TodayClosingCount = _context.CashRegisterClosings
                .Count(c => c.ClosingDate >= today && c.ClosingDate < tomorrow);

            ViewBag.PendingCount = _context.CashRegisterClosings
                .Count(c => c.Status == "Beklemede");

            ViewBag.ApprovedCount = _context.CashRegisterClosings
                .Count(c => c.Status == "Onaylandı");

            ViewBag.RejectedCount = _context.CashRegisterClosings
                .Count(c => c.Status == "Reddedildi");

            ViewBag.TotalExpectedCash = closings
                .Sum(c => c.CashSalesTotal);

            ViewBag.TotalDeclaredCash = closings
                .Sum(c => c.DeclaredCashAmount);

            ViewBag.TotalCashDifference = closings
                .Sum(c => c.CashDifference);

            return View(closings);
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

            var todaySales = _context.Sales.ActiveSales()
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

            var todaySales = _context.Sales.ActiveSales()
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

            TempData["Success"] = "Kasa kapanışı oluşturuldu. Onay için yöneticiye gönderildi.";

            return RedirectToAction("MyClosings");
        }

        [HttpPost]
        [PermissionAuthorize("sale.view.all", "sale.view.branch", "role.manage")]
        public IActionResult Approve(int id, string? reviewNote)
        {
            var closing = _context.CashRegisterClosings.Find(id);

            if (closing == null)
            {
                TempData["Error"] = "Kasa kapanışı bulunamadı.";
                return RedirectToAction("Index");
            }

            if (closing.Status != "Beklemede")
            {
                TempData["Error"] = "Sadece bekleyen kasa kapanışları onaylanabilir.";
                return RedirectToAction("Index");
            }

            closing.Status = "Onaylandı";
            closing.ReviewedAt = DateTime.Now;
            closing.ReviewNote = reviewNote;

            _context.SaveChanges();

            TempData["Success"] = "Kasa kapanışı onaylandı.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [PermissionAuthorize("sale.view.all", "sale.view.branch", "role.manage")]
        public IActionResult Reject(int id, string? reviewNote)
        {
            var closing = _context.CashRegisterClosings.Find(id);

            if (closing == null)
            {
                TempData["Error"] = "Kasa kapanışı bulunamadı.";
                return RedirectToAction("Index");
            }

            if (closing.Status != "Beklemede")
            {
                TempData["Error"] = "Sadece bekleyen kasa kapanışları reddedilebilir.";
                return RedirectToAction("Index");
            }

            closing.Status = "Reddedildi";
            closing.ReviewedAt = DateTime.Now;
            closing.ReviewNote = reviewNote;

            _context.SaveChanges();

            TempData["Success"] = "Kasa kapanışı reddedildi.";

            return RedirectToAction("Index");
        }
    }
}
