using MarketERP.Data;
using MarketERP.Models;
using MarketERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketERP.Controllers
{
    [PermissionAuthorize("category.view")]
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

            var categories = _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .OrderBy(c => c.ParentCategoryId == null ? 0 : 1)
                .ThenBy(c => c.ParentCategory != null ? c.ParentCategory.Name : c.Name)
                .ThenBy(c => c.Name)
                .ToList();

            var parentCategories = categories
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.Name)
                .ToList();

            ViewBag.ParentCategories = parentCategories;

            ViewBag.TotalCategoryCount = categories.Count;
            ViewBag.MainCategoryCount = categories.Count(c => c.ParentCategoryId == null);
            ViewBag.SubCategoryCount = categories.Count(c => c.ParentCategoryId != null);
            ViewBag.UncategorizedProductCount = _context.Products.Count(p => p.CategoryId == null);

            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("category.manage")]
        public IActionResult Add(string name, int? parentCategoryId, string? defaultVatRateText)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Kategori adı boş olamaz.";
                return RedirectToAction("Index");
            }

            name = name.Trim();

            bool sameCategoryExists = _context.Categories.Any(c =>
                c.Name.ToLower() == name.ToLower() &&
                c.ParentCategoryId == parentCategoryId);

            if (sameCategoryExists)
            {
                TempData["Error"] = "Bu kategori aynı ana kategori altında zaten var.";
                return RedirectToAction("Index");
            }

            if (parentCategoryId != null)
            {
                bool parentExists = _context.Categories.Any(c =>
                    c.Id == parentCategoryId.Value &&
                    c.ParentCategoryId == null);

                if (!parentExists)
                {
                    TempData["Error"] = "Alt kategori eklemek için geçerli bir ana kategori seçmelisiniz.";
                    return RedirectToAction("Index");
                }
            }

            decimal? defaultVatRate = ParseNullableDecimal(defaultVatRateText);

            Category category = new Category
            {
                Name = name,
                ParentCategoryId = parentCategoryId,
                DefaultVatRate = defaultVatRate
            };

            _context.Categories.Add(category);
            _context.SaveChanges();

            TempData["Success"] = "Kategori başarıyla eklendi.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("category.manage")]
        public IActionResult Update(int id, string name, int? parentCategoryId, string? defaultVatRateText)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var category = _context.Categories.Find(id);

            if (category == null)
            {
                TempData["Error"] = "Kategori bulunamadı.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Kategori adı boş olamaz.";
                return RedirectToAction("Index");
            }

            name = name.Trim();

            if (parentCategoryId == id)
            {
                TempData["Error"] = "Bir kategori kendi ana kategorisi olamaz.";
                return RedirectToAction("Index");
            }

            if (parentCategoryId != null)
            {
                bool parentExists = _context.Categories.Any(c =>
                    c.Id == parentCategoryId.Value &&
                    c.ParentCategoryId == null);

                if (!parentExists)
                {
                    TempData["Error"] = "Ana kategori olarak sadece üst seviye kategoriler seçilebilir.";
                    return RedirectToAction("Index");
                }
            }

            bool sameCategoryExists = _context.Categories.Any(c =>
                c.Id != id &&
                c.Name.ToLower() == name.ToLower() &&
                c.ParentCategoryId == parentCategoryId);

            if (sameCategoryExists)
            {
                TempData["Error"] = "Bu kategori aynı ana kategori altında zaten var.";
                return RedirectToAction("Index");
            }

            category.Name = name;
            category.ParentCategoryId = parentCategoryId;
            category.DefaultVatRate = ParseNullableDecimal(defaultVatRateText);

            _context.SaveChanges();

            TempData["Success"] = "Kategori başarıyla güncellendi.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("category.manage")]
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var category = _context.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                TempData["Error"] = "Kategori bulunamadı.";
                return RedirectToAction("Index");
            }

            bool hasSubCategories = _context.Categories.Any(c => c.ParentCategoryId == id);
            bool hasProducts = _context.Products.Any(p => p.CategoryId == id);

            if (hasSubCategories || hasProducts)
            {
                TempData["Error"] = "Bu kategoriye bağlı ürün veya alt kategori olduğu için silinemez.";
                return RedirectToAction("Index");
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();

            TempData["Success"] = "Kategori başarıyla silindi.";

            return RedirectToAction("Index");
        }

        private decimal? ParseNullableDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            value = value.Trim();

            if (decimal.TryParse(
                value,
                System.Globalization.NumberStyles.Any,
                new System.Globalization.CultureInfo("tr-TR"),
                out decimal trResult))
            {
                return trResult;
            }

            if (decimal.TryParse(
                value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal invariantResult))
            {
                return invariantResult;
            }

            return null;
        }
    }
}
