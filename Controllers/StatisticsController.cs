using MarketERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace MarketERP.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public StatisticsController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.ProductCount = _context.Products.Count();
            ViewBag.CategoryCount = _context.Categories.Count();
            ViewBag.CustomerCount = _context.Customers.Count();
            ViewBag.EmployeeCount = _context.Employees.Count();
            ViewBag.SaleCount = _context.Sales.Count();
            ViewBag.TotalRevenue = _context.Sales.Sum(s => s.TotalAmount);
            ViewBag.TotalBonus = _context.EmployeeBonuses.Sum(b => b.BonusAmount);
            ViewBag.OpenTicketCount = _context.SupportTickets.Count(t => t.Status == "Açık");
            ViewBag.ResolvedTicketCount = _context.SupportTickets.Count(t => t.Status == "Çözüldü");
            ViewBag.PendingLeaveCount = _context.EmployeeLeaves.Count(l => l.Status == "Beklemede");

            ViewBag.CriticalProducts = _context.Products
                .Where(p => p.StockQuantity <= p.CriticalStock)
                .ToList();

            ViewBag.LastSales = _context.Sales
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .ToList();

            ViewBag.TopSellingProducts = _context.SaleDetails
                .Include(s => s.Product)
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

            ViewBag.TableCount = GetTableCount();
            ViewBag.TableNames = GetTableNames();

            return View();
        }

        private int GetTableCount()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            string query = @"
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = 'market_erp'";

            using var command = new MySqlCommand(query, connection);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        private List<string> GetTableNames()
        {
            var tables = new List<string>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            string query = @"
                SELECT TABLE_NAME
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = 'market_erp'
                ORDER BY TABLE_NAME";

            using var command = new MySqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                tables.Add(reader.GetString("TABLE_NAME"));
            }

            return tables;
        }
    }
}