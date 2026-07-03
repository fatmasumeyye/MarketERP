using MarketERP.Data;
using MarketERP.Models;
using MarketERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketERP.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly AppDbContext _context;

        public SuppliersController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string status = "active")
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            status = NormalizeStatusFilter(status);
            var query = _context.Suppliers.AsQueryable();
            if (status == "active") query = query.Where(s => s.IsActive);
            else if (status == "inactive") query = query.Where(s => !s.IsActive);

            ViewBag.Status = status;
            var suppliers = query.OrderBy(s => s.CompanyName).ToList();
            return View(suppliers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("supplier.manage")]
        public IActionResult Add(Supplier supplier)
        {
            supplier.IsActive = true;
            _context.Suppliers.Add(supplier);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("supplier.manage")]
        public IActionResult Delete(int id)
        {
            var supplier = _context.Suppliers.Find(id);

            if (supplier == null)
            {
                TempData["Error"] = "Tedarikçi bulunamadı.";
                return RedirectToAction("Index");
            }

            if (!supplier.IsActive)
            {
                TempData["Info"] = "Tedarikçi zaten pasif durumda.";
                return RedirectToAction("Index", new { status = "inactive" });
            }

            supplier.IsActive = false;
            _context.SaveChanges();
            TempData["Success"] = "Tedarikçi pasif duruma alındı. Geçmiş siparişleri korunuyor.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("supplier.manage")]
        public IActionResult Activate(int id)
        {
            var supplier = _context.Suppliers.Find(id);
            if (supplier == null)
            {
                TempData["Error"] = "Tedarikçi bulunamadı.";
                return RedirectToAction("Index", new { status = "inactive" });
            }

            if (supplier.IsActive)
            {
                TempData["Info"] = "Tedarikçi zaten aktif durumda.";
                return RedirectToAction("Index");
            }

            supplier.IsActive = true;
            _context.SaveChanges();
            TempData["Success"] = "Tedarikçi yeniden aktifleştirildi.";
            return RedirectToAction("Index", new { status = "inactive" });
        }

        [PermissionAuthorize("supplier.view")]
        public async Task<IActionResult> Details(int id)
        {
            var supplier = await _context.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (supplier == null) return NotFound();
            var orders = _context.PurchaseOrders.AsNoTracking()
                .Where(o => o.SupplierId == id && o.Status != "İptal Edildi");

            return View(new SupplierDetailViewModel
            {
                Supplier = supplier,
                TotalPurchases = await orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
                TotalOrders = await orders.CountAsync(),
                LastOrder = await orders.OrderByDescending(o => o.OrderDate).FirstOrDefaultAsync()
            });
        }

        [PermissionAuthorize("supplier.manage")]
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            return supplier == null ? NotFound() : View(supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("supplier.manage")]
        public async Task<IActionResult> Edit(Supplier model)
        {
            var supplier = await _context.Suppliers.FindAsync(model.Id);
            if (supplier == null) return NotFound();
            if (string.IsNullOrWhiteSpace(model.CompanyName)) ModelState.AddModelError(nameof(model.CompanyName), "Firma adı zorunludur.");
            if (!ModelState.IsValid) return View(model);

            supplier.CompanyName = model.CompanyName.Trim();
            supplier.Phone = model.Phone?.Trim() ?? string.Empty;
            supplier.Email = model.Email?.Trim() ?? string.Empty;
            supplier.Address = model.Address?.Trim() ?? string.Empty;
            supplier.DiscountRate = Math.Clamp(model.DiscountRate, 0, 100);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Tedarikçi bilgileri güncellendi.";
            return RedirectToAction(nameof(Details), new { id = supplier.Id });
        }

        private static string NormalizeStatusFilter(string? status)
        {
            return status is "all" or "inactive" ? status : "active";
        }
    }
}
