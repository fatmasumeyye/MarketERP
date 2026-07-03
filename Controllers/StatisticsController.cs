using MarketERP.Data;
using MarketERP.Helpers;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketERP.Controllers
{
    [PermissionAuthorize("role.manage")]
    public class StatisticsController : Controller
    {
        private readonly AppDbContext _context;

        public StatisticsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            ViewBag.TodayRevenue = _context.Sales.ActiveSales()
                .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            ViewBag.MonthlyRevenue = _context.Sales.ActiveSales()
                .Where(s => s.SaleDate >= monthStart && s.SaleDate < tomorrow)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            var saleCount = _context.Sales.ActiveSales().Count();
            ViewBag.SaleCount = saleCount;
            ViewBag.CustomerCount = _context.Customers.Count();

            ViewBag.CriticalStockCount = _context.Products
                .Count(p => p.StockQuantity > 0 && p.StockQuantity <= p.CriticalStock);

            ViewBag.OutOfStockCount = _context.Products
                .Count(p => p.StockQuantity <= 0);

            ViewBag.NormalStockCount = _context.Products
                .Count(p => p.StockQuantity > p.CriticalStock);

            var returnRequestCount = _context.ReturnRequests
                .Select(r => r.RequestNo)
                .Distinct()
                .Count();

            ViewBag.PendingReturnRequestCount = _context.ReturnRequests
                .Where(r => r.Status == "Beklemede" || r.Status == "Onay Bekliyor")
                .Select(r => r.RequestNo)
                .Distinct()
                .Count();

            ViewBag.ReturnRequestCount = returnRequestCount;
            ViewBag.ReturnRate = saleCount > 0
                ? Math.Round((decimal)returnRequestCount / saleCount * 100, 2)
                : 0;

            ViewBag.PendingWholesaleRequestCount = _context.WholesaleSaleRequests
                .Count(r => r.Status == "Beklemede" || r.Status == "Onay Bekliyor");

            ViewBag.PendingCashClosingCount = _context.CashRegisterClosings
                .Count(c => c.Status == "Beklemede");

            ViewBag.PendingLeaveCount = _context.EmployeeLeaves
                .Count(l => l.Status == "Beklemede" || l.Status == "Onay Bekliyor");

            ViewBag.CriticalProducts = _context.Products
                .Where(p => p.StockQuantity > 0 && p.StockQuantity <= p.CriticalStock)
                .OrderBy(p => p.StockQuantity)
                .Take(10)
                .ToList();

            ViewBag.OutOfStockProducts = _context.Products
                .Where(p => p.StockQuantity <= 0)
                .OrderBy(p => p.Name)
                .Take(10)
                .ToList();

            ViewBag.RecentSales = _context.Sales.ActiveSales()
                .Include(s => s.Customer)
                .Include(s => s.Employee)
                .OrderByDescending(s => s.SaleDate)
                .Take(10)
                .ToList();

            var topSellingProducts = _context.SaleDetails
                .Where(sd => sd.Product != null
                    && sd.Sale != null
                    && sd.Sale.Status != Sale.CancelledStatus)
                .GroupBy(sd => sd.Product!.Name)
                .Select(g => new
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Subtotal)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(10)
                .ToList();

            ViewBag.TopSellingProducts = topSellingProducts
                .Select(x => Tuple.Create(x.ProductName, x.TotalQuantity, x.TotalRevenue))
                .ToList();

            var paymentTypeDistribution = _context.Sales.ActiveSales()
                .GroupBy(s => s.PaymentType == null || s.PaymentType == ""
                    ? "Belirtilmedi"
                    : s.PaymentType)
                .Select(g => new
                {
                    PaymentType = g.Key,
                    TotalAmount = g.Sum(x => x.TotalAmount)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            ViewBag.PaymentTypeDistribution = paymentTypeDistribution
                .Select(x => Tuple.Create(x.PaymentType, x.TotalAmount))
                .ToList();

            var topCustomers = _context.Sales.ActiveSales()
                .Where(s => s.CustomerId != null)
                .GroupBy(s => new
                {
                    s.CustomerId,
                    CustomerName = s.Customer != null
                        ? s.Customer.FullName
                        : "Bilinmeyen Müşteri"
                })
                .Select(g => new
                {
                    g.Key.CustomerName,
                    SaleCount = g.Count(),
                    TotalAmount = g.Sum(x => x.TotalAmount)
                })
                .OrderByDescending(x => x.TotalAmount)
                .Take(10)
                .ToList();

            ViewBag.TopCustomers = topCustomers
                .Select(x => Tuple.Create(x.CustomerName, x.SaleCount, x.TotalAmount))
                .ToList();

            var chartStart = today.AddDays(-29);
            var dailySales = _context.Sales.ActiveSales()
                .Where(s => s.SaleDate >= chartStart && s.SaleDate < tomorrow)
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new
                {
                    Day = g.Key,
                    Revenue = g.Sum(x => x.TotalAmount),
                    SaleCount = g.Count()
                })
                .ToList()
                .ToDictionary(x => x.Day);

            var dailySalesLabels = new List<string>();
            var dailySalesRevenue = new List<decimal>();
            var dailySalesCounts = new List<int>();

            for (var i = 0; i < 30; i++)
            {
                var day = chartStart.AddDays(i);
                dailySales.TryGetValue(day, out var dailyValue);

                dailySalesLabels.Add(day.ToString("dd.MM"));
                dailySalesRevenue.Add(dailyValue?.Revenue ?? 0);
                dailySalesCounts.Add(dailyValue?.SaleCount ?? 0);
            }

            ViewBag.DailySalesLabels = dailySalesLabels;
            ViewBag.DailySalesRevenue = dailySalesRevenue;
            ViewBag.DailySalesCounts = dailySalesCounts;
            ViewBag.HasDailySalesData = dailySalesRevenue.Any(x => x > 0);

            var categorySales = _context.SaleDetails
                .Where(sd => sd.Sale != null && sd.Sale.Status != Sale.CancelledStatus)
                .GroupBy(sd => sd.Product != null && sd.Product.Category != null
                    ? sd.Product.Category.Name
                    : "Kategorisiz")
                .Select(g => new
                {
                    CategoryName = g.Key,
                    Revenue = g.Sum(x => x.Subtotal)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToList();

            ViewBag.CategorySalesLabels = categorySales.Select(x => x.CategoryName).ToList();
            ViewBag.CategorySalesData = categorySales.Select(x => x.Revenue).ToList();
            ViewBag.HasCategorySalesData = categorySales.Count > 0;

            ViewBag.TopProductChartLabels = topSellingProducts
                .Select(x => x.ProductName)
                .ToList();
            ViewBag.TopProductChartData = topSellingProducts
                .Select(x => x.TotalQuantity)
                .ToList();
            ViewBag.HasTopProductData = topSellingProducts.Count > 0;

            ViewBag.PaymentTypeChartLabels = paymentTypeDistribution
                .Select(x => x.PaymentType)
                .ToList();
            ViewBag.PaymentTypeChartData = paymentTypeDistribution
                .Select(x => x.TotalAmount)
                .ToList();
            ViewBag.HasPaymentTypeData = paymentTypeDistribution.Count > 0;

            var returnStatusData = _context.ReturnRequests
                .Select(r => new { r.RequestNo, r.Status })
                .ToList()
                .GroupBy(r => string.IsNullOrWhiteSpace(r.Status) ? "Belirtilmedi" : r.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Select(x => x.RequestNo).Distinct().Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            ViewBag.ReturnStatusLabels = returnStatusData.Select(x => x.Status).ToList();
            ViewBag.ReturnStatusData = returnStatusData.Select(x => x.Count).ToList();
            ViewBag.HasReturnStatusData = returnStatusData.Count > 0;

            ViewBag.StockStatusLabels = new[] { "Normal Stok", "Kritik Stok", "Stokta Biten" };
            ViewBag.StockStatusData = new[]
            {
                (int)ViewBag.NormalStockCount,
                (int)ViewBag.CriticalStockCount,
                (int)ViewBag.OutOfStockCount
            };
            ViewBag.HasStockStatusData = _context.Products.Any();

            return View();
        }
    }
}
