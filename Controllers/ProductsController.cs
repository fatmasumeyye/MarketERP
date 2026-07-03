using MarketERP.Data;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using MarketERP.Helpers;
using MarketERP.Services;
using System.Data;
using System.Text.Json;

namespace MarketERP.Controllers
{
    [PermissionAuthorize("product.view")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IStockMovementService _stockMovementService;

        public ProductsController(
            AppDbContext context,
            IStockMovementService stockMovementService)
        {
            _context = context;
            _stockMovementService = stockMovementService;
        }

        public IActionResult Index(
            string search,
            int? categoryId,
            int? supplierId,
            string stockStatus,
            string recordStatus = "active")
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

            recordStatus = NormalizeStatusFilter(recordStatus);
            if (recordStatus == "active") products = products.Where(p => p.IsActive);
            else if (recordStatus == "inactive") products = products.Where(p => !p.IsActive);

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
            else if (stockStatus == "out")
            {
                products = products.Where(p => p.StockQuantity <= 0);
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
            ViewBag.RecordStatus = recordStatus;

            ViewBag.ProductIdsUsedInSales = _context.SaleDetails
                .Select(sd => sd.ProductId)
                .Distinct()
                .ToList();

            return View(products.OrderBy(p => p.Name).ToList());
        }

        [HttpGet]
        [PermissionAuthorize("stock.view", "stock.adjust", "product.update", "role.manage")]
        public async Task<IActionResult> StockReconciliation(string status = "all")
        {
            status = status?.Trim().ToLowerInvariant() switch
            {
                "matched" => "matched",
                "difference" => "difference",
                "missing" => "missing",
                _ => "all"
            };

            var ledgerTotals = await _context.StockMovements
                .AsNoTracking()
                .GroupBy(movement => movement.ProductId)
                .Select(group => new
                {
                    ProductId = group.Key,
                    MovementCount = group.Count(),
                    CalculatedQuantity = group.Sum(movement =>
                        movement.MovementType == StockMovementService.InboundMovement
                            ? movement.Quantity
                            : movement.MovementType == StockMovementService.OutboundMovement
                                ? -movement.Quantity
                                : 0)
                })
                .ToDictionaryAsync(item => item.ProductId);

            var products = await _context.Products
                .AsNoTracking()
                .OrderBy(product => product.Name)
                .Select(product => new
                {
                    product.Id,
                    product.Name,
                    product.Barcode,
                    product.StockQuantity
                })
                .ToListAsync();

            var allItems = products.Select(product =>
            {
                bool hasLedger = ledgerTotals.TryGetValue(product.Id, out var ledger);
                return new StockReconciliationItemViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Barcode = product.Barcode,
                    CurrentQuantity = product.StockQuantity,
                    LedgerQuantity = hasLedger ? ledger!.CalculatedQuantity : 0,
                    HasLedger = hasLedger
                };
            }).ToList();

            var filteredItems = status switch
            {
                "matched" => allItems.Where(item => item.HasLedger && item.Difference == 0).ToList(),
                "difference" => allItems.Where(item => item.HasLedger && item.Difference != 0).ToList(),
                "missing" => allItems.Where(item => !item.HasLedger).ToList(),
                _ => allItems
            };

            return View(new StockReconciliationViewModel
            {
                Items = filteredItems,
                StatusFilter = status,
                TotalProductCount = allItems.Count,
                MatchedCount = allItems.Count(item => item.HasLedger && item.Difference == 0),
                DifferenceCount = allItems.Count(item => item.HasLedger && item.Difference != 0),
                MissingLedgerCount = allItems.Count(item => !item.HasLedger)
            });
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
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("product.create")]
        public async Task<IActionResult> Create(Product product)
        {
            if (!IsValidSubCategory(product.CategoryId))
            {
                TempData["Error"] = "Ürün eklemek için ana kategori değil, alt kategori seçmelisiniz.";
                return RedirectToAction("Create");
            }

            if (product.StockQuantity < 0)
            {
                TempData["Error"] = "Başlangıç stok miktarı negatif olamaz.";
                return RedirectToAction("Create");
            }

            if (string.IsNullOrWhiteSpace(product.Barcode)
                || string.IsNullOrWhiteSpace(product.Name))
            {
                TempData["Error"] = "Barkod ve ürün adı zorunludur.";
                return RedirectToAction("Create");
            }

            string barcode = product.Barcode.Trim();
            int initialQuantity = product.StockQuantity;
            int? employeeId = HttpContext.Session.GetInt32("EmployeeId");

            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

            try
            {
                var existingProduct = await _context.Products
                    .FromSqlInterpolated($"SELECT * FROM products WHERE barcode = {barcode} FOR UPDATE")
                    .SingleOrDefaultAsync();

                if (existingProduct != null)
                {
                    if (!string.Equals(
                            existingProduct.Name.Trim(),
                            product.Name.Trim(),
                            StringComparison.OrdinalIgnoreCase))
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = "Bu barkod başka bir ürüne ait. Aynı barkodla farklı ürün eklenemez.";
                        return RedirectToAction("Create");
                    }

                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.SupplierId = product.SupplierId;
                    existingProduct.PurchasePrice = product.PurchasePrice;
                    existingProduct.SalePrice = product.SalePrice;
                    existingProduct.CriticalStock = product.CriticalStock;

                    if (initialQuantity > 0)
                    {
                        await _stockMovementService.RecordAsync(new StockMovementCommand(
                            ProductId: existingProduct.Id,
                            MovementType: StockMovementService.InboundMovement,
                            ReasonType: "ManuelGiris",
                            Quantity: initialQuantity,
                            MovementDate: DateTime.Now,
                            SourceType: "Product",
                            SourceId: existingProduct.Id,
                            SourceNo: barcode,
                            Description: "Aynı barkod üzerinden stok girişi.",
                            CreatedByEmployeeId: employeeId));
                    }
                    else
                    {
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    TempData["Success"] = initialQuantity > 0
                        ? "Ürün bilgileri güncellendi ve stok girişi hareket defterine işlendi."
                        : "Ürün bilgileri güncellendi; stok miktarı değişmedi.";
                    return RedirectToAction("Index");
                }

                product.Barcode = barcode;
                product.Name = product.Name.Trim();
                product.StockQuantity = 0;
                product.CreatedAt = DateTime.Now;
                product.IsActive = true;

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                if (initialQuantity > 0)
                {
                    await _stockMovementService.RecordAsync(new StockMovementCommand(
                        ProductId: product.Id,
                        MovementType: StockMovementService.InboundMovement,
                        ReasonType: "AcilisBakiyesi",
                        Quantity: initialQuantity,
                        MovementDate: DateTime.Now,
                        SourceType: "Product",
                        SourceId: product.Id,
                        SourceNo: string.IsNullOrWhiteSpace(product.Barcode) ? product.Name : product.Barcode,
                        Description: "Yeni ürün başlangıç stok bakiyesi.",
                        CreatedByEmployeeId: employeeId));
                }

                await transaction.CommitAsync();
                TempData["Success"] = initialQuantity > 0
                    ? "Ürün oluşturuldu ve başlangıç stoğu hareket defterine işlendi."
                    : "Ürün başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Ürün kaydedilemedi; ürün ve stok hareketlerinde değişiklik yapılmadı.";
                return RedirectToAction("Create");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("product.delete")]
        public IActionResult Delete(int id)
        {
            if (!HttpContext.HasPermission("product.delete"))
            {
                return RedirectToAction("AccessDenied", "Login");
            }

            var product = _context.Products.Find(id);
            if (product == null)
            {
                TempData["Error"] = "Ürün bulunamadı.";
                return RedirectToAction("Index");
            }

            if (!product.IsActive)
            {
                TempData["Info"] = "Ürün zaten pasif durumda.";
                return RedirectToAction("Index", new { recordStatus = "inactive" });
            }

            product.IsActive = false;
            _context.SaveChanges();
            TempData["Success"] = "Ürün pasif duruma alındı. Satış ve stok geçmişi korunuyor.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("product.delete")]
        public IActionResult Activate(int id)
        {
            if (!HttpContext.HasPermission("product.delete"))
            {
                return RedirectToAction("AccessDenied", "Login");
            }

            var product = _context.Products.Find(id);
            if (product == null)
            {
                TempData["Error"] = "Ürün bulunamadı.";
                return RedirectToAction("Index", new { recordStatus = "inactive" });
            }

            if (product.IsActive)
            {
                TempData["Info"] = "Ürün zaten aktif durumda.";
                return RedirectToAction("Index");
            }

            product.IsActive = true;
            _context.SaveChanges();
            TempData["Success"] = "Ürün yeniden aktifleştirildi.";
            return RedirectToAction("Index", new { recordStatus = "inactive" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("stock.adjust")]
        public async Task<IActionResult> AddStock(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                TempData["Error"] = "Eklenecek stok miktarı 0'dan büyük olmalıdır.";
                return RedirectToAction("Index");
            }

            try
            {
                await _stockMovementService.RecordAsync(new StockMovementCommand(
                    ProductId: productId,
                    MovementType: StockMovementService.InboundMovement,
                    ReasonType: "ManuelGiris",
                    Quantity: quantity,
                    MovementDate: DateTime.Now,
                    SourceType: "Product",
                    SourceId: productId,
                    SourceNo: $"PRD-{productId:D6}",
                    Description: "Ürünler ekranından manuel stok girişi.",
                    CreatedByEmployeeId: HttpContext.Session.GetInt32("EmployeeId")));
            }
            catch (InvalidOperationException exception)
            {
                TempData["Error"] = exception.Message;
                return RedirectToAction("Index");
            }

            TempData["Success"] = "Stok girişi kaydedildi ve stok hareket defterine işlendi.";
            return RedirectToAction("Index");
        }

        [PermissionAuthorize("product.update")]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = GetSubCategorySelectList(product.CategoryId);
            ViewBag.Suppliers = new SelectList(
                _context.Suppliers
                    .Where(s => s.IsActive || s.Id == product.SupplierId)
                    .OrderBy(s => s.CompanyName)
                    .ToList(),
                "Id",
                "CompanyName",
                product.SupplierId
            );

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("product.update")]
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

            if (product.SupplierId != existingProduct.SupplierId &&
                product.SupplierId.HasValue &&
                !_context.Suppliers.Any(s => s.Id == product.SupplierId.Value && s.IsActive))
            {
                TempData["Error"] = "Pasif tedarikçi ürüne atanamaz.";
                return RedirectToAction("Edit", new { id = product.Id });
            }

            existingProduct.Name = product.Name;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.SupplierId = product.SupplierId;
            existingProduct.PurchasePrice = product.PurchasePrice;
            existingProduct.SalePrice = product.SalePrice;
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
                _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.CompanyName).ToList(),
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

        private static string NormalizeStatusFilter(string? status)
        {
            return status is "all" or "inactive" ? status : "active";
        }
    }
}
