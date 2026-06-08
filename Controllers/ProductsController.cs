using MarketERP.Data;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using MarketERP.Helpers;
using System.Text.Json;

namespace MarketERP.Controllers
{
    [PermissionAuthorize("product.view")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search, int? categoryId, int? supplierId, string stockStatus)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            PrepareCategoryAndSupplierViewBags(supplierId);

            var products = _context.Products
                .Include(p => p.Category)
                    .ThenInclude(c => c.ParentCategory)
                .Include(p => p.Supplier)
                .AsQueryable();

            string selectedCategoryName = "Tüm Ürünler";

            if (categoryId != null)
            {
                var selectedCategory = _context.Categories
                    .FirstOrDefault(c => c.Id == categoryId.Value);

                if (selectedCategory != null)
                {
                    selectedCategoryName = selectedCategory.Name;

                    if (selectedCategory.ParentCategoryId == null)
                    {
                        products = products.Where(p =>
                            p.Category != null &&
                            p.Category.ParentCategoryId == selectedCategory.Id);
                    }
                    else
                    {
                        products = products.Where(p => p.CategoryId == selectedCategory.Id);
                    }
                }
            }

            if (supplierId != null)
            {
                products = products.Where(p => p.SupplierId == supplierId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.Where(p =>
                    p.Name.Contains(search) ||
                    p.Barcode.Contains(search));
            }

            if (stockStatus == "critical")
            {
                products = products.Where(p => p.StockQuantity <= p.CriticalStock);
            }
            else if (stockStatus == "low")
            {
                products = products.Where(p =>
                    p.StockQuantity > p.CriticalStock &&
                    p.StockQuantity <= p.CriticalStock + 5);
            }
            else if (stockStatus == "normal")
            {
                products = products.Where(p => p.StockQuantity > p.CriticalStock + 5);
            }

            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedCategoryName = selectedCategoryName;
            ViewBag.SelectedSupplierId = supplierId;
            ViewBag.Search = search;
            ViewBag.StockStatus = stockStatus;

            ViewBag.ProductIdsUsedInSales = _context.SaleDetails
                .Select(sd => sd.ProductId)
                .Distinct()
                .ToList();

            return View(products.OrderBy(p => p.Name).ToList());
        }

        [PermissionAuthorize("product.create")]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            PrepareCategoryAndSupplierViewBags();

            return View();
        }

        [HttpPost]
        [PermissionAuthorize("product.create")]
        public IActionResult Create(Product product)
        {
            if (!IsValidSubCategory(product.CategoryId))
            {
                TempData["Error"] = "Ürün eklemek için ana kategori değil, alt kategori seçmelisiniz.";
                return RedirectToAction("Create");
            }

            var existingProduct = _context.Products
                .FirstOrDefault(p => p.Barcode == product.Barcode);

            if (existingProduct != null)
            {
                if (existingProduct.Name.ToLower() != product.Name.ToLower())
                {
                    TempData["Error"] = "Bu barkod başka bir ürüne ait. Aynı barkodla farklı ürün eklenemez.";
                    return RedirectToAction("Create");
                }

                existingProduct.CategoryId = product.CategoryId;
                existingProduct.SupplierId = product.SupplierId;
                existingProduct.PurchasePrice = product.PurchasePrice;
                existingProduct.SalePrice = product.SalePrice;
                existingProduct.StockQuantity += product.StockQuantity;
                existingProduct.CriticalStock = product.CriticalStock;

                _context.SaveChanges();

                TempData["Success"] = "Ürün zaten vardı. Bilgileri güncellendi ve stok miktarı artırıldı.";
                return RedirectToAction("Index");
            }

            product.CreatedAt = DateTime.Now;

            _context.Products.Add(product);
            _context.SaveChanges();

            TempData["Success"] = "Ürün başarıyla eklendi.";
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            if (!HttpContext.HasPermission("product.delete"))
            {
                return RedirectToAction("AccessDenied", "Login");
            }

            var hasSaleDetails = _context.SaleDetails.Any(sd => sd.ProductId == id);

            if (hasSaleDetails)
            {
                TempData["Error"] = "Bu ürün daha önce satışta kullanıldığı için silinemez. ERP sistemlerinde satış geçmişi olan ürünler silinmez.";
                return RedirectToAction("Index");
            }

            var product = _context.Products.Find(id);

            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();

                TempData["Success"] = "Ürün başarıyla silindi.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AddStock(int productId, int quantity)
        {
            var product = _context.Products.Find(productId);

            if (product == null)
            {
                TempData["Error"] = "Ürün bulunamadı.";
                return RedirectToAction("Index");
            }

            if (quantity <= 0)
            {
                TempData["Error"] = "Eklenecek stok miktarı 0'dan büyük olmalıdır.";
                return RedirectToAction("Index");
            }

            product.StockQuantity += quantity;
            _context.SaveChanges();

            TempData["Success"] = "Stok başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = GetSubCategorySelectList(product.CategoryId);
            ViewBag.Suppliers = new SelectList(
                _context.Suppliers.ToList(),
                "Id",
                "CompanyName",
                product.SupplierId
            );

            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            var existingProduct = _context.Products.Find(product.Id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            if (!IsValidSubCategory(product.CategoryId))
            {
                TempData["Error"] = "Ürün için ana kategori değil, KDV oranı belli olan alt kategori seçmelisiniz.";
                return RedirectToAction("Edit", new { id = product.Id });
            }

            existingProduct.Name = product.Name;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.SupplierId = product.SupplierId;
            existingProduct.PurchasePrice = product.PurchasePrice;
            existingProduct.SalePrice = product.SalePrice;
            existingProduct.StockQuantity = product.StockQuantity;
            existingProduct.CriticalStock = product.CriticalStock;

            _context.SaveChanges();

            TempData["Success"] = "Ürün başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        private void PrepareCategoryAndSupplierViewBags(int? selectedSupplierId = null)
        {
            var mainCategories = _context.Categories
                .Include(c => c.SubCategories)
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.Name)
                .ToList();

            ViewBag.MainCategories = mainCategories;

            var subCategories = _context.Categories
                .Include(c => c.ParentCategory)
                .Where(c => c.ParentCategoryId != null && c.DefaultVatRate != null)
                .OrderBy(c => c.ParentCategory.Name)
                .ThenBy(c => c.Name)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    parentId = c.ParentCategoryId
                })
                .ToList();

            ViewBag.SubCategoriesJson = JsonSerializer.Serialize(subCategories);

            ViewBag.Suppliers = new SelectList(
                _context.Suppliers.OrderBy(s => s.CompanyName).ToList(),
                "Id",
                "CompanyName",
                selectedSupplierId
            );
        }

        private SelectList GetSubCategorySelectList(int? selectedCategoryId = null)
        {
            var categories = _context.Categories
                .Include(c => c.ParentCategory)
                .Where(c => c.ParentCategoryId != null && c.DefaultVatRate != null)
                .OrderBy(c => c.ParentCategory.Name)
                .ThenBy(c => c.Name)
                .Select(c => new
                {
                    c.Id,
                    DisplayName = c.ParentCategory.Name + " > " + c.Name
                })
                .ToList();

            return new SelectList(categories, "Id", "DisplayName", selectedCategoryId);
        }

        private bool IsValidSubCategory(int? categoryId)
        {
            if (categoryId == null)
                return false;

            return _context.Categories.Any(c =>
                c.Id == categoryId.Value &&
                c.ParentCategoryId != null &&
                c.DefaultVatRate != null);
        }
    }
}