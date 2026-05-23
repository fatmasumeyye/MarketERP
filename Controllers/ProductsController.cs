using MarketERP.Data;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using MarketERP.Helpers;

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

        public IActionResult Index(string search)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
            ViewBag.Suppliers = new SelectList(_context.Suppliers.ToList(), "Id", "CompanyName");

            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.Where(p => p.Name.Contains(search));
            }

            ViewBag.ProductIdsUsedInSales = _context.SaleDetails
    .Select(sd => sd.ProductId)
    .Distinct()
    .ToList();

            return View(products.ToList());
        }

        [HttpPost]
        public IActionResult Add(Product product)
        {
            var existingProduct = _context.Products
                .FirstOrDefault(p => p.Barcode == product.Barcode);

            if (existingProduct != null)
            {
                // Aynı barkod farklı ürüne aitse hata ver
                if (existingProduct.Name.ToLower() != product.Name.ToLower())
                {
                    TempData["Error"] = "Bu barkod başka bir ürüne ait. Aynı barkodla farklı ürün eklenemez.";
                    return RedirectToAction("Index");
                }

                // Aynı ürünse güncelle
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.SupplierId = product.SupplierId;
                existingProduct.PurchasePrice = product.PurchasePrice;
                existingProduct.SalePrice = product.SalePrice;
                existingProduct.StockQuantity += product.StockQuantity;
                existingProduct.CriticalStock = product.CriticalStock;

                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            product.CreatedAt = DateTime.Now;

            _context.Products.Add(product);
            _context.SaveChanges();

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
                return Content("Ürün bulunamadı.");
            }

            if (quantity <= 0)
            {
                return Content("Eklenecek stok miktarı 0'dan büyük olmalıdır.");
            }

            product.StockQuantity += quantity;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
            ViewBag.Suppliers = new SelectList(_context.Suppliers.ToList(), "Id", "CompanyName");

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

            existingProduct.Name = product.Name;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.SupplierId = product.SupplierId;
            existingProduct.PurchasePrice = product.PurchasePrice;
            existingProduct.SalePrice = product.SalePrice;
            existingProduct.StockQuantity = product.StockQuantity;
            existingProduct.CriticalStock = product.CriticalStock;

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}