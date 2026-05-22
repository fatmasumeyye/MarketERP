using MarketERP.Data;
using Microsoft.AspNetCore.Mvc;

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

            var today = DateTime.Today;
            var currentMonth = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;

            ViewBag.CategoryCount = _context.Categories.Count();
            ViewBag.ProductCount = _context.Products.Count();
            ViewBag.CustomerCount = _context.Customers.Count();

            ViewBag.TotalRevenue = _context.Sales.Sum(s => s.TotalAmount);
            ViewBag.SaleCount = _context.Sales.Count();

            ViewBag.CriticalStockCount = _context.Products
                .Count(p => p.StockQuantity <= p.CriticalStock);

            ViewBag.TodaySaleCount = _context.Sales
                .Count(s => s.SaleDate.Date == today);

            ViewBag.TodayRevenue = _context.Sales
                .Where(s => s.SaleDate.Date == today)
                .Sum(s => s.TotalAmount);
            ViewBag.MonthlyRevenue = _context.Sales
    .Where(s => s.SaleDate.Month == currentMonth
             && s.SaleDate.Year == currentYear)
    .Sum(s => s.TotalAmount);

            ViewBag.YearlyRevenue = _context.Sales
                .Where(s => s.SaleDate.Year == currentYear)
                .Sum(s => s.TotalAmount);

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

            ViewBag.TotalBonus = _context.EmployeeBonuses.Sum(b => b.BonusAmount);

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

        public IActionResult Privacy()
        {
            return View();
        }
    }
}