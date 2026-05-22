using MarketERP.Data;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;

namespace MarketERP.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var categories = _context.Categories.ToList();
            return View(categories);
        }

        [HttpPost]
        public IActionResult Add(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Category category = new Category
                {
                    Name = name
                };

                _context.Categories.Add(category);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var category = _context.Categories.Find(id);

            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}