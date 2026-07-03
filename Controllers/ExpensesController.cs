using System.Globalization;
using MarketERP.Data;
using MarketERP.Helpers;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;

namespace MarketERP.Controllers
{
    [PermissionAuthorize("finance.view")]
    public class ExpensesController : Controller
    {
        private static readonly string[] ExpenseCategories =
        {
            "Kira",
            "Elektrik faturası",
            "Su faturası",
            "Doğalgaz faturası",
            "İnternet faturası",
            "Personel maaşı",
            "SGK / sigorta gideri",
            "Vergi",
            "Tedarikçi ödemesi",
            "Bakım / onarım",
            "Temizlik gideri",
            "Kargo / lojistik",
            "Ofis / kırtasiye",
            "Diğer giderler"
        };

        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var chartStart = new DateTime(today.Year, today.Month, 1).AddMonths(-11);

            var expenses = _context.Expenses
                .OrderByDescending(e => e.ExpenseDate)
                .ToList();

            var totalIncome = _context.Sales.ActiveSales()
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;
            var totalExpense = expenses.Sum(e => e.Amount);

            var monthlyIncome = _context.Sales.ActiveSales()
                .Where(s => s.SaleDate >= monthStart && s.SaleDate < tomorrow)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;
            var monthlyExpense = expenses
                .Where(e => e.ExpenseDate >= monthStart && e.ExpenseDate < tomorrow)
                .Sum(e => e.Amount);

            var wholesaleSaleIds = _context.WholesaleSaleRequests
                .Where(w => w.SaleId != null)
                .Select(w => w.SaleId!.Value);

            var wholesaleIncome = _context.Sales.ActiveSales()
                .Where(s => wholesaleSaleIds.Contains(s.Id))
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            ViewBag.TotalIncome = totalIncome;
            ViewBag.TotalExpense = totalExpense;
            ViewBag.NetProfit = totalIncome - totalExpense;
            ViewBag.MonthlyIncome = monthlyIncome;
            ViewBag.MonthlyExpense = monthlyExpense;
            ViewBag.MonthlyNet = monthlyIncome - monthlyExpense;
            ViewBag.RetailIncome = totalIncome - wholesaleIncome;
            ViewBag.WholesaleIncome = wholesaleIncome;
            ViewBag.OtherIncome = 0m;
            ViewBag.ExpenseCategories = ExpenseCategories;

            var expenseCategoryData = expenses
                .GroupBy(GetExpenseCategory)
                .Select(g => new
                {
                    Category = g.Key,
                    Amount = g.Sum(x => x.Amount)
                })
                .OrderByDescending(x => x.Amount)
                .ToList();

            ViewBag.ExpenseCategoryLabels = expenseCategoryData
                .Select(x => x.Category)
                .ToList();
            ViewBag.ExpenseCategoryData = expenseCategoryData
                .Select(x => x.Amount)
                .ToList();
            ViewBag.ExpenseCategorySummary = expenseCategoryData
                .Select(x => Tuple.Create(x.Category, x.Amount))
                .ToList();
            ViewBag.HasExpenseCategoryData = expenseCategoryData.Count > 0;

            var monthlySalesData = _context.Sales.ActiveSales()
                .Where(s => s.SaleDate >= chartStart && s.SaleDate < tomorrow)
                .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Amount = g.Sum(x => x.TotalAmount)
                })
                .ToList()
                .ToDictionary(x => (x.Year, x.Month), x => x.Amount);

            var monthlyExpenseData = expenses
                .Where(e => e.ExpenseDate >= chartStart && e.ExpenseDate < tomorrow)
                .GroupBy(e => (e.ExpenseDate.Year, e.ExpenseDate.Month))
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

            var culture = CultureInfo.GetCultureInfo("tr-TR");
            var monthlyLabels = new List<string>();
            var monthlyIncomeData = new List<decimal>();
            var monthlyExpensesData = new List<decimal>();

            for (var i = 0; i < 12; i++)
            {
                var month = chartStart.AddMonths(i);
                var key = (month.Year, month.Month);

                monthlyLabels.Add(month.ToString("MMM yyyy", culture));
                monthlyIncomeData.Add(monthlySalesData.GetValueOrDefault(key));
                monthlyExpensesData.Add(monthlyExpenseData.GetValueOrDefault(key));
            }

            ViewBag.MonthlyChartLabels = monthlyLabels;
            ViewBag.MonthlyIncomeData = monthlyIncomeData;
            ViewBag.MonthlyExpenseData = monthlyExpensesData;
            ViewBag.HasMonthlyFinanceData =
                monthlyIncomeData.Any(x => x > 0)
                || monthlyExpensesData.Any(x => x > 0);

            return View(expenses.Take(15).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Expense expense)
        {
            if (string.IsNullOrWhiteSpace(expense.Title)
                || expense.Amount <= 0
                || expense.ExpenseDate == default)
            {
                TempData["Error"] = "Gider kategorisi, pozitif tutar ve tarih zorunludur.";
                return RedirectToAction("Index");
            }

            expense.Title = expense.Title.Trim();
            expense.Description = expense.Description?.Trim() ?? string.Empty;

            _context.Expenses.Add(expense);
            _context.SaveChanges();

            TempData["Success"] = "Gider kaydı eklendi.";
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

        private static string GetExpenseCategory(Expense expense)
        {
            var text = $"{expense.Title} {expense.Description}"
                .ToLower(CultureInfo.GetCultureInfo("tr-TR"));

            if (text.Contains("elektrik")) return "Elektrik faturası";
            if (text.Contains("doğalgaz") || text.Contains("dogalgaz")) return "Doğalgaz faturası";
            if (text.Contains("internet")) return "İnternet faturası";
            if (text.Contains("sgk") || text.Contains("sigorta")) return "SGK / sigorta gideri";
            if (text.Contains("maaş") || text.Contains("maas") || text.Contains("personel")) return "Personel maaşı";
            if (text.Contains("vergi")) return "Vergi";
            if (text.Contains("tedarikçi") || text.Contains("tedarikci")) return "Tedarikçi ödemesi";
            if (text.Contains("bakım") || text.Contains("bakim") || text.Contains("onarım") || text.Contains("onarim")) return "Bakım / onarım";
            if (text.Contains("temizlik")) return "Temizlik gideri";
            if (text.Contains("kargo") || text.Contains("lojistik") || text.Contains("nakliye")) return "Kargo / lojistik";
            if (text.Contains("ofis") || text.Contains("kırtasiye") || text.Contains("kirtasiye")) return "Ofis / kırtasiye";
            if (text.Contains("kira")) return "Kira";
            if (text.Contains("su fatur") || expense.Title.Equals("Su", StringComparison.OrdinalIgnoreCase)) return "Su faturası";

            return "Diğer giderler";
        }
    }
}
