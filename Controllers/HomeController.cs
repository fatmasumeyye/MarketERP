using MarketERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketERP.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var roles = HttpContext.Session.GetString("Roles");

            if (!string.IsNullOrEmpty(roles) && roles.Contains("Kasiyer"))
            {
                return CashierDashboard();
            }

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var currentMonth = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;

            ViewBag.CategoryCount = _context.Categories.Count();
            ViewBag.ProductCount = _context.Products.Count();
            ViewBag.CustomerCount = _context.Customers.Count();

            ViewBag.TotalRevenue = _context.Sales
                .Select(s => (decimal?)s.TotalAmount)
                .Sum() ?? 0;

            ViewBag.SaleCount = _context.Sales.Count();

            ViewBag.CriticalStockCount = _context.Products
                .Count(p => p.StockQuantity <= p.CriticalStock);

            ViewBag.TodaySaleCount = _context.Sales
                .Count(s => s.SaleDate >= today && s.SaleDate < tomorrow);

            ViewBag.TodayRevenue = _context.Sales
                .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow)
                .Select(s => (decimal?)s.TotalAmount)
                .Sum() ?? 0;

            ViewBag.MonthlyRevenue = _context.Sales
                .Where(s => s.SaleDate.Month == currentMonth
                         && s.SaleDate.Year == currentYear)
                .Select(s => (decimal?)s.TotalAmount)
                .Sum() ?? 0;

            ViewBag.YearlyRevenue = _context.Sales
                .Where(s => s.SaleDate.Year == currentYear)
                .Select(s => (decimal?)s.TotalAmount)
                .Sum() ?? 0;

            ViewBag.LastSales = _context.Sales
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .ToList();

            ViewBag.TopSellingProducts = _context.SaleDetails
                .GroupBy(s => s.Product.Name)
                .Select(g => new
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Subtotal)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToList();

            ViewBag.CriticalProducts = _context.Products
                .Where(p => p.StockQuantity <= p.CriticalStock)
                .ToList();

            ViewBag.EmployeeCount = _context.Employees.Count();

            ViewBag.TotalBonus = _context.EmployeeBonuses
                .Select(b => (decimal?)b.BonusAmount)
                .Sum() ?? 0;

            ViewBag.PendingLeaveCount = _context.EmployeeLeaves
                .Count(l => l.Status == "Beklemede");

            ViewBag.LastEmployees = _context.Employees
                .OrderByDescending(e => e.Id)
                .Take(5)
                .ToList();

            ViewBag.OpenTicketCount = _context.SupportTickets
                .Count(t => t.Status == "Açık");

            ViewBag.ResolvedTicketCount = _context.SupportTickets
                .Count(t => t.Status == "Çözüldü");

            ViewBag.ChartLabels = new string[]
            {
                "Ürün",
                "Müşteri",
                "Satış",
                "Çalışan"
            };

            ViewBag.ChartData = new int[]
            {
                _context.Products.Count(),
                _context.Customers.Count(),
                _context.Sales.Count(),
                _context.Employees.Count()
            };

            return View();
        }

        private IActionResult CashierDashboard()
        {
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            var fullName = HttpContext.Session.GetString("FullName");

            if (employeeId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var todaySales = _context.Sales
                .Where(s =>
                    s.EmployeeId == employeeId.Value &&
                    s.SaleDate >= today &&
                    s.SaleDate < tomorrow);

            var todaySaleCount = todaySales.Count();

            var todayTotal = todaySales
                .Select(s => (decimal?)s.TotalAmount)
                .Sum() ?? 0;

            var averageSaleAmount = todaySaleCount > 0
                ? todayTotal / todaySaleCount
                : 0;

            var lastSale = _context.Sales
                .Where(s => s.EmployeeId == employeeId.Value)
                .OrderByDescending(s => s.SaleDate)
                .FirstOrDefault();

            ViewBag.FullName = fullName;
            ViewBag.TodaySaleCount = todaySaleCount;
            ViewBag.TodayTotal = todayTotal;
            ViewBag.AverageSaleAmount = averageSaleAmount;
            ViewBag.LastSale = lastSale;

            ViewBag.LastMySales = _context.Sales
                .Include(s => s.Customer)
                .Where(s => s.EmployeeId == employeeId.Value)
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .ToList();

            return View("CashierDashboard");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}