using MarketERP.Data;
using MarketERP.Helpers;
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

            var roleNames = (HttpContext.Session.GetString("Roles") ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var isAdmin = roleNames.Contains("Admin", StringComparer.OrdinalIgnoreCase);
            var isStoreManager = roleNames.Contains(
                "Mağaza Müdürü",
                StringComparer.OrdinalIgnoreCase);
            var isCashier = roleNames.Contains("Kasiyer", StringComparer.OrdinalIgnoreCase);
            var isWholesaleSalesStaff = roleNames.Contains(
                "Toptan Satış Sorumlusu",
                StringComparer.OrdinalIgnoreCase);
            var isManagementRole = isAdmin || isStoreManager;
            var isWarehouseManager =
                roleNames.Contains("Depo Sorumlusu", StringComparer.OrdinalIgnoreCase)
                || (
                    !isManagementRole
                    && HttpContext.HasPermission("stock.view")
                    && HttpContext.HasPermission("stock.count")
                    && HttpContext.HasPermission("stock.transfer")
                    && HttpContext.HasPermission("stock.adjust")
                );
            var isAccounting =
                roleNames.Contains("Muhasebe", StringComparer.OrdinalIgnoreCase)
                || (
                    !isManagementRole
                    && !isWarehouseManager
                    && HttpContext.HasPermission("finance.view")
                    && HttpContext.HasPermission("reports.financial")
                );

            var isWholesaleStaff =
                isWholesaleSalesStaff
                && HttpContext.HasPermission("sale.wholesale.create")
                && !isManagementRole;

            var isCashierOrSalesStaff =
                isCashier
                && (
                    HttpContext.HasPermission("sale.retail.create")
                    || HttpContext.HasPermission("sale.view.own")
                    || HttpContext.HasPermission("cash.closing.create")
                    || HttpContext.HasPermission("return.request")
                )
                && !isManagementRole
                && !isWholesaleStaff;

            if (isWarehouseManager)
            {
                return WarehouseDashboard();
            }

            if (isAccounting)
            {
                return AccountingDashboard();
            }

            if (isWholesaleStaff)
            {
                return WholesaleDashboard();
            }

            if (isCashierOrSalesStaff)
            {
                return CashierDashboard();
            }

            ViewBag.IsStoreManager = isStoreManager;
            ViewBag.IsAdmin = isAdmin;

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
                .Count(p => p.StockQuantity > 0 && p.StockQuantity <= p.CriticalStock);

            ViewBag.OutOfStockCount = _context.Products
                .Count(p => p.StockQuantity <= 0);

            // Admin Bekleyen İşlem Kartları
            ViewBag.PendingLeavesCount = _context.EmployeeLeaves
                .Count(l => l.Status == "Beklemede" || l.Status == "Onay Bekliyor");

            ViewBag.PendingSupportTicketsCount = _context.SupportTickets
                .Count(t => t.Status == "Açık" || t.Status == "Beklemede");

            ViewBag.PendingReturnRequestsCount = _context.ReturnRequests
                .Where(r => r.Status == "Beklemede" || r.Status == "Onay Bekliyor")
                .Select(r => r.RequestNo)
                .Distinct()
                .Count();

            ViewBag.PendingCashClosingCount = _context.CashRegisterClosings
                .Count(c => c.Status == "Beklemede");

            ViewBag.PendingWholesaleRequestCount = _context.WholesaleSaleRequests
                .Count(r => r.Status == "Beklemede" || r.Status == "Onay Bekliyor");

            // EmployeeBonus modelinde Status alanı olmadığı için şimdilik toplam prim kaydı gösteriyoruz.
            ViewBag.PendingBonusesCount = _context.EmployeeBonuses.Count();

            // Kritik stok ürünleri
            ViewBag.LowStockProducts = _context.Products
                .Where(p => p.StockQuantity > 0 && p.StockQuantity <= p.CriticalStock)
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

            // Bugün en çok satan 5 ürün
            var topSellingProducts = _context.SaleDetails
                .Where(sd =>
                    sd.Product != null
                    && sd.Sale != null
                    && sd.Sale.SaleDate >= today
                    && sd.Sale.SaleDate < tomorrow)
                .GroupBy(sd => sd.Product!.Name)
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

        private IActionResult WarehouseDashboard()
        {
            ViewBag.ProductCount = _context.Products.Count();
            ViewBag.CriticalStockCount = _context.Products
                .Count(p => p.StockQuantity > 0 && p.StockQuantity <= p.CriticalStock);
            ViewBag.OutOfStockCount = _context.Products
                .Count(p => p.StockQuantity <= 0);
            ViewBag.TotalStockQuantity = _context.Products
                .Sum(p => (int?)p.StockQuantity) ?? 0;
            ViewBag.SupplierCount = _context.Suppliers.Count();

            ViewBag.PendingPurchaseOrderCount = _context.PurchaseOrders
                .Count(o => o.Status == "Onay Bekliyor");

            ViewBag.AwaitingReceiptOrderCount = _context.PurchaseOrders
                .Count(o =>
                    o.Status == "Onaylandı"
                    || o.Status == "Kısmi Teslim Alındı");

            ViewBag.LowestStockProducts = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .OrderBy(p => p.StockQuantity)
                .ThenBy(p => p.Name)
                .Take(8)
                .ToList();

            ViewBag.HighestStockProducts = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.StockQuantity)
                .ThenBy(p => p.Name)
                .Take(5)
                .ToList();

            ViewBag.RecentPurchaseOrders = _context.PurchaseOrders
                .AsNoTracking()
                .Include(o => o.Supplier)
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .Take(8)
                .ToList();

            return View("WarehouseDashboard");
        }

        private IActionResult AccountingDashboard()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var todayRevenue = _context.Sales
                .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            var monthlyRevenue = _context.Sales
                .Where(s => s.SaleDate >= monthStart && s.SaleDate < tomorrow)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            var monthlyExpense = _context.Expenses
                .Where(e => e.ExpenseDate >= monthStart && e.ExpenseDate < tomorrow)
                .Sum(e => (decimal?)e.Amount) ?? 0;

            ViewBag.TodayRevenue = todayRevenue;
            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.TotalCollections = _context.Sales
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;
            ViewBag.MonthlyExpense = monthlyExpense;
            ViewBag.MonthlyNet = monthlyRevenue - monthlyExpense;

            ViewBag.PendingCashClosingCount = _context.CashRegisterClosings
                .Count(c => c.Status == "Beklemede");
            ViewBag.ApprovedCashClosingCount = _context.CashRegisterClosings
                .Count(c => c.Status == "Onaylandı");

            ViewBag.PendingReturnCount = _context.ReturnRequests
                .Where(r => r.Status == "Beklemede" || r.Status == "Onay Bekliyor")
                .Select(r => r.RequestNo)
                .Distinct()
                .Count();

            var pendingWholesaleRequests = _context.WholesaleSaleRequests
                .Where(r => r.Status == "Beklemede" || r.Status == "Onay Bekliyor");
            ViewBag.PendingWholesaleCount = pendingWholesaleRequests.Count();
            ViewBag.PendingWholesaleAmount = pendingWholesaleRequests
                .Sum(r => (decimal?)r.TotalAmount) ?? 0;

            var pendingPurchaseOrders = _context.PurchaseOrders
                .Where(o =>
                    o.Status == "Taslak"
                    || o.Status == "Beklemede"
                    || o.Status == "Onay Bekliyor");
            ViewBag.PendingPurchaseOrderCount = pendingPurchaseOrders.Count();
            ViewBag.PendingPurchaseOrderAmount = pendingPurchaseOrders
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            var paymentTypeTotals = _context.Sales
                .Where(s => s.SaleDate >= monthStart && s.SaleDate < tomorrow)
                .GroupBy(s => string.IsNullOrWhiteSpace(s.PaymentType)
                    ? "Belirtilmedi"
                    : s.PaymentType)
                .Select(g => new
                {
                    PaymentType = g.Key,
                    TotalAmount = g.Sum(s => s.TotalAmount)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            ViewBag.PaymentTypeSummary = paymentTypeTotals
                .Select(x => new KeyValuePair<string, decimal>(
                    x.PaymentType,
                    x.TotalAmount))
                .ToList();

            ViewBag.RecentCashClosings = _context.CashRegisterClosings
                .Include(c => c.Employee)
                .OrderByDescending(c => c.ClosingDate)
                .Take(5)
                .ToList();

            ViewBag.RecentExpenses = _context.Expenses
                .OrderByDescending(e => e.ExpenseDate)
                .Take(5)
                .ToList();

            ViewBag.RecentSales = _context.Sales
                .Include(s => s.Customer)
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .ToList();

            return View("AccountingDashboard");
        }

        private IActionResult CashierDashboard()
        {
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (employeeId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            var todaySales = _context.Sales
                .Include(s => s.Customer)
                .Where(s =>
                    s.EmployeeId == employeeId.Value &&
                    s.SaleDate >= today &&
                    s.SaleDate < tomorrow)
                .ToList();

            ViewBag.TodaySalesCount = todaySales.Count;
            ViewBag.TodayRevenue = todaySales.Sum(s => s.TotalAmount);

            ViewBag.TodayCashRevenue = todaySales
                .Where(s => s.PaymentType == "Nakit")
                .Sum(s => s.TotalAmount);

            ViewBag.TodayCardRevenue = todaySales
                .Where(s => s.PaymentType == "Kart")
                .Sum(s => s.TotalAmount);

            ViewBag.LastSales = _context.Sales
                .Include(s => s.Customer)
                .Where(s => s.EmployeeId == employeeId.Value)
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .ToList();

            var todayClosing = _context.CashRegisterClosings
                .FirstOrDefault(c =>
                    c.EmployeeId == employeeId.Value &&
                    c.ClosingDate >= today &&
                    c.ClosingDate < tomorrow);

            ViewBag.TodayClosing = todayClosing;
            ViewBag.HasTodayClosing = todayClosing != null;

            ViewBag.MyPendingReturnCount = _context.ReturnRequests
                .Count(r =>
                    r.EmployeeId == employeeId.Value &&
                    r.Status == "Beklemede");

            ViewBag.MyTotalSalesCount = _context.Sales
                .Count(s => s.EmployeeId == employeeId.Value);

            ViewBag.MyTotalRevenue = _context.Sales
                .Where(s => s.EmployeeId == employeeId.Value)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            return View("CashierDashboard");
        }

        private IActionResult WholesaleDashboard()
        {
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (employeeId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);
            DateTime monthStart = new DateTime(today.Year, today.Month, 1);

            var myRequests = _context.WholesaleSaleRequests
                .Include(r => r.Customer)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Product)
                .Where(r => r.EmployeeId == employeeId.Value)
                .ToList();

            ViewBag.MyTotalWholesaleRequestCount = myRequests.Count;

            ViewBag.MyPendingWholesaleRequestCount = myRequests
                .Count(r => r.Status == "Onay Bekliyor" || r.Status == "Beklemede");

            ViewBag.MyApprovedWholesaleRequestCount = myRequests
                .Count(r => r.Status == "Onaylandı");

            ViewBag.MyRejectedWholesaleRequestCount = myRequests
                .Count(r => r.Status == "Reddedildi");

            ViewBag.TodayWholesaleRequestCount = myRequests
                .Count(r => r.RequestDate >= today && r.RequestDate < tomorrow);

            ViewBag.MonthlyWholesaleAmount = myRequests
                .Where(r => r.RequestDate >= monthStart && r.RequestDate < tomorrow)
                .Sum(r => (decimal?)r.TotalAmount) ?? 0;

            ViewBag.MyRecentWholesaleRequests = myRequests
                .OrderByDescending(r => r.RequestDate)
                .Take(5)
                .ToList();

            ViewBag.MyPendingWholesaleRequests = myRequests
                .Where(r => r.Status == "Onay Bekliyor" || r.Status == "Beklemede")
                .OrderByDescending(r => r.RequestDate)
                .Take(5)
                .ToList();

            DateTime sevenDaysAgo = today.AddDays(-6);

            var chartLabels = new List<string>();
            var chartData = new List<decimal>();

            for (int i = 0; i < 7; i++)
            {
                DateTime day = sevenDaysAgo.AddDays(i);
                DateTime nextDay = day.AddDays(1);

                decimal dayTotal = myRequests
                    .Where(r => r.RequestDate >= day && r.RequestDate < nextDay)
                    .Sum(r => r.TotalAmount);

                chartLabels.Add(day.ToString("dd.MM"));
                chartData.Add(dayTotal);
            }

            ViewBag.WholesaleAmountChartLabels = chartLabels;
            ViewBag.WholesaleAmountChartData = chartData;

            return View("WholesaleDashboard");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
