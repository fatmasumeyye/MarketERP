using MarketERP.Data;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;
using MarketERP.Models;
using MarketERP.Data;
using MarketERP.Helpers;

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
        public IActionResult Execute(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                ViewBag.Error = "SQL sorgusu boş olamaz.";
                return View("Index");
            }

            string upperQuery = query.ToUpper();

            if (upperQuery.Contains("DROP") ||
                upperQuery.Contains("TRUNCATE"))
            {
                ViewBag.Error = "Tehlikeli SQL komutlarına izin verilmez.";
                return View("Index");
            }

            var table = new DataTable();

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

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
                return View("Index");
            }
        }
        [HttpPost]
        public IActionResult SaveQuery(string title, string query)
        {
            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(query))
            {
                TempData["Error"] = "Başlık ve sorgu boş olamaz.";
                return RedirectToAction("Index");
            }

            var savedQuery = new SavedQuery
            {
                Title = title,
                SqlQuery = query
            };

            _context.SavedQueries.Add(savedQuery);
            _context.SaveChanges();

            TempData["Success"] = "Sorgu kaydedildi.";

            return RedirectToAction("Index");
        }
        public IActionResult SavedQueries()
        {
            var queries = _context.SavedQueries.ToList();
            return View(queries);
        }

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