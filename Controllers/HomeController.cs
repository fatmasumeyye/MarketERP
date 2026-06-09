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

            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            DateTime weekStart = today.AddDays(-((int)today.DayOfWeek == 0 ? 6 : (int)today.DayOfWeek - 1));
            DateTime monthStart = new DateTime(today.Year, today.Month, 1);
            DateTime yearStart = new DateTime(today.Year, 1, 1);

            // Satış / Ciro Kartları
            ViewBag.TodaySalesCount = _context.Sales
                .Count(s => s.SaleDate >= today && s.SaleDate < tomorrow);

            ViewBag.TodayRevenue = _context.Sales
                .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            ViewBag.WeeklyRevenue = _context.Sales
                .Where(s => s.SaleDate >= weekStart && s.SaleDate < tomorrow)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            ViewBag.MonthlyRevenue = _context.Sales
                .Where(s => s.SaleDate >= monthStart && s.SaleDate < tomorrow)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            ViewBag.YearlyRevenue = _context.Sales
                .Where(s => s.SaleDate >= yearStart && s.SaleDate < tomorrow)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            // Genel Sistem Kartları
            ViewBag.ProductCount = _context.Products.Count();

            ViewBag.CriticalStockCount = _context.Products
                .Count(p => p.StockQuantity <= p.CriticalStock);

            // Admin Bekleyen İşlem Kartları
            ViewBag.PendingLeavesCount = _context.EmployeeLeaves
                .Count(l => l.Status == "Beklemede" || l.Status == "Onay Bekliyor");

            ViewBag.PendingSupportTicketsCount = _context.SupportTickets
                .Count(t => t.Status == "Açık" || t.Status == "Beklemede");

            ViewBag.PendingReturnRequestsCount = _context.ReturnRequests
                .Count(r => r.Status == "Beklemede" || r.Status == "Onay Bekliyor");

            // EmployeeBonus modelinde Status alanı olmadığı için şimdilik toplam prim kaydı gösteriyoruz.
            ViewBag.PendingBonusesCount = _context.EmployeeBonuses.Count();

            // Kritik stok ürünleri
            ViewBag.LowStockProducts = _context.Products
                .Where(p => p.StockQuantity <= p.CriticalStock)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .ToList();

            // Son satışlar
            ViewBag.RecentSales = _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Employee)
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .ToList();

            // Son 7 günlük ciro ve satış adedi grafikleri
            DateTime sevenDaysAgo = today.AddDays(-6);

            var last7DaysLabels = new List<string>();
            var last7DaysRevenue = new List<decimal>();
            var last7DaysSalesCount = new List<int>();

            for (int i = 0; i < 7; i++)
            {
                DateTime day = sevenDaysAgo.AddDays(i);
                DateTime nextDay = day.AddDays(1);

                decimal dayRevenue = _context.Sales
                    .Where(s => s.SaleDate >= day && s.SaleDate < nextDay)
                    .Sum(s => (decimal?)s.TotalAmount) ?? 0;

                int daySalesCount = _context.Sales
                    .Count(s => s.SaleDate >= day && s.SaleDate < nextDay);

                last7DaysLabels.Add(day.ToString("dd.MM"));
                last7DaysRevenue.Add(dayRevenue);
                last7DaysSalesCount.Add(daySalesCount);
            }

            ViewBag.RevenueChartLabels = last7DaysLabels;
            ViewBag.RevenueChartData = last7DaysRevenue;
            ViewBag.SalesCountChartData = last7DaysSalesCount;

            // Bu ay ödeme tipi dağılımı
            var paymentTypeData = _context.Sales
                .Where(s => s.SaleDate >= monthStart && s.SaleDate < tomorrow)
                .GroupBy(s => string.IsNullOrWhiteSpace(s.PaymentType) ? "Belirtilmedi" : s.PaymentType)
                .Select(g => new
                {
                    PaymentType = g.Key,
                    TotalAmount = g.Sum(x => x.TotalAmount)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            ViewBag.PaymentTypeLabels = paymentTypeData
                .Select(x => x.PaymentType)
                .ToList();

            ViewBag.PaymentTypeData = paymentTypeData
                .Select(x => x.TotalAmount)
                .ToList();

            // En çok satan 5 ürün
            var topSellingProducts = _context.SaleDetails
                .Include(sd => sd.Product)
                .Where(sd => sd.Product != null)
                .GroupBy(sd => sd.Product.Name)
                .Select(g => new
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToList();

            ViewBag.TopProductLabels = topSellingProducts
                .Select(x => x.ProductName)
                .ToList();

            ViewBag.TopProductData = topSellingProducts
                .Select(x => x.TotalQuantity)
                .ToList();

            // Eski view tarafında kalmış olabilecek isimler hata vermesin diye bırakıyoruz.
            ViewBag.CategoryCount = _context.Categories.Count();
            ViewBag.CustomerCount = _context.Customers.Count();
            ViewBag.EmployeeCount = _context.Employees.Count();

            ViewBag.TotalRevenue = _context.Sales
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            ViewBag.SaleCount = _context.Sales.Count();

            ViewBag.TotalBonus = _context.EmployeeBonuses
                .Sum(b => (decimal?)b.BonusAmount) ?? 0;

            ViewBag.OpenTicketCount = _context.SupportTickets
                .Count(t => t.Status == "Açık");

            ViewBag.ResolvedTicketCount = _context.SupportTickets
                .Count(t => t.Status == "Çözüldü");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}