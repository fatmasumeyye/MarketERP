using MarketERP.Data;
using MarketERP.Helpers;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Data;

namespace MarketERP.Controllers
{
    [PermissionAuthorize("sql.editor")]
    public class SqlEditorController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public SqlEditorController(
            IConfiguration configuration,
            AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public IActionResult Index(string query)
        {
            ViewBag.Query = query;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Execute(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                ViewBag.Error = "SQL sorgusu boş olamaz.";
                return View("Index");
            }

            var normalizedQuery = query.TrimStart();
            var upperQuery = normalizedQuery.ToUpperInvariant();
            var queryWithoutTrailingSemicolon = normalizedQuery
                .TrimEnd()
                .TrimEnd(';')
                .TrimEnd();
            var isReadOnlyQuery =
                upperQuery.StartsWith("SELECT ")
                || upperQuery.StartsWith("SELECT\n")
                || upperQuery.StartsWith("SHOW ")
                || upperQuery.StartsWith("SHOW\n")
                || upperQuery.StartsWith("DESCRIBE ")
                || upperQuery.StartsWith("DESCRIBE\n")
                || upperQuery.StartsWith("DESC ")
                || upperQuery.StartsWith("DESC\n")
                || upperQuery.StartsWith("EXPLAIN ")
                || upperQuery.StartsWith("EXPLAIN\n");

            if (!isReadOnlyQuery
                || queryWithoutTrailingSemicolon.Contains(';')
                || upperQuery.Contains("INTO OUTFILE")
                || upperQuery.Contains("INTO DUMPFILE")
                || upperQuery.Contains("LOAD_FILE("))
            {
                ViewBag.Error = "Yalnızca SELECT, SHOW, DESCRIBE ve EXPLAIN sorgularına izin verilir.";
                ViewBag.Query = query;
                return View("Index");
            }

            var table = new DataTable();

            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                using var command = new MySqlCommand(query, connection);
                using var adapter = new MySqlDataAdapter(command);
                adapter.Fill(table);

                ViewBag.Query = query;
                return View("Result", table);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Query = query;
                return View("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveQuery(string title, string query)
        {
            if (string.IsNullOrWhiteSpace(title)
                || string.IsNullOrWhiteSpace(query))
            {
                TempData["Error"] = "Başlık ve sorgu boş olamaz.";
                return RedirectToAction("Index");
            }

            var savedQuery = new SavedQuery
            {
                Title = title.Trim(),
                SqlQuery = query.Trim()
            };

            _context.SavedQueries.Add(savedQuery);
            _context.SaveChanges();

            TempData["Success"] = "Sorgu kaydedildi.";
            return RedirectToAction("Index");
        }

        public IActionResult SavedQueries()
        {
            var queries = _context.SavedQueries
                .OrderBy(q => q.Title)
                .ToList();

            return View(queries);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSavedQuery(int id)
        {
            var query = _context.SavedQueries.Find(id);

            if (query != null)
            {
                _context.SavedQueries.Remove(query);
                _context.SaveChanges();
            }

            return RedirectToAction("SavedQueries");
        }
    }
}
