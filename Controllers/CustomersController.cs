using System.Globalization;
using MarketERP.Data;
using MarketERP.Helpers;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketERP.Controllers
{
    public class CustomersController : Controller
    {
        private readonly AppDbContext _context;

        public CustomersController(AppDbContext context)
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
            var customers = _context.Customers.AsQueryable();

            if (status == "active") customers = customers.Where(c => c.IsActive);
            else if (status == "inactive") customers = customers.Where(c => !c.IsActive);

            ViewBag.Status = status;
            var customerList = customers
                .OrderBy(c => c.FullName)
                .ToList();

            return View(customerList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("customer.create")]
        public IActionResult Add(Customer customer)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var canManageDiscount =
                HttpContext.HasPermission("sale.wholesale.create")
                || HttpContext.HasPermission("role.manage")
                || HttpContext.HasPermission("user.manage")
                || HttpContext.HasPermission("sale.view.all");

            if (canManageDiscount)
            {
                var discountRateText = Request.Form["DiscountRate"].ToString();

                if (!string.IsNullOrWhiteSpace(discountRateText))
                {
                    discountRateText = discountRateText.Replace(",", ".");

                    if (decimal.TryParse(
                            discountRateText,
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out decimal parsedDiscountRate))
                    {
                        customer.DiscountRate = parsedDiscountRate;
                    }
                    else
                    {
                        customer.DiscountRate = 0;
                    }
                }
                else
                {
                    customer.DiscountRate = 0;
                }
            }
            else
            {
                customer.DiscountRate = 0;
            }

            if (customer.DiscountRate < 0)
            {
                customer.DiscountRate = 0;
            }

            if (customer.DiscountRate > 100)
            {
                customer.DiscountRate = 100;
            }

            customer.IsActive = true;
            _context.Customers.Add(customer);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateDiscount(int id, string discountRate)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var canManageDiscount =
                HttpContext.HasPermission("sale.wholesale.create")
                || HttpContext.HasPermission("role.manage")
                || HttpContext.HasPermission("user.manage")
                || HttpContext.HasPermission("sale.view.all");

            if (!canManageDiscount)
            {
                return RedirectToAction("Index");
            }

            var customer = _context.Customers.Find(id);

            if (customer == null)
            {
                return RedirectToAction("Index");
            }

            decimal parsedDiscountRate = 0;

            if (!string.IsNullOrWhiteSpace(discountRate))
            {
                discountRate = discountRate.Replace(",", ".");

                decimal.TryParse(
                    discountRate,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out parsedDiscountRate);
            }

            if (parsedDiscountRate < 0)
            {
                parsedDiscountRate = 0;
            }

            if (parsedDiscountRate > 100)
            {
                parsedDiscountRate = 100;
            }

            customer.DiscountRate = parsedDiscountRate;

            _context.Customers.Update(customer);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("customer.update")]
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var customer = _context.Customers.Find(id);

            if (customer == null)
            {
                TempData["Error"] = "Müşteri bulunamadı.";
                return RedirectToAction("Index");
            }

            if (customer.FullName == "Nihai Tüketici")
            {
                TempData["Error"] = "Sistem müşterisi Nihai Tüketici pasif yapılamaz.";
                return RedirectToAction("Index");
            }

            if (!customer.IsActive)
            {
                TempData["Info"] = "Müşteri zaten pasif durumda.";
                return RedirectToAction("Index", new { status = "inactive" });
            }

            customer.IsActive = false;
            _context.SaveChanges();
            TempData["Success"] = "Müşteri pasif duruma alındı. Geçmiş kayıtları korunuyor.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("customer.update")]
        public IActionResult Activate(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null)
            {
                TempData["Error"] = "Müşteri bulunamadı.";
                return RedirectToAction("Index", new { status = "inactive" });
            }

            if (customer.IsActive)
            {
                TempData["Info"] = "Müşteri zaten aktif durumda.";
                return RedirectToAction("Index");
            }

            customer.IsActive = true;
            _context.SaveChanges();
            TempData["Success"] = "Müşteri yeniden aktifleştirildi.";
            return RedirectToAction("Index", new { status = "inactive" });
        }

        public IActionResult DiscountedCustomers()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var customers = _context.Customers
                .Where(c => c.IsActive && c.DiscountRate > 0)
                .OrderByDescending(c => c.DiscountRate)
                .ThenBy(c => c.FullName)
                .ToList();

            return View(customers);
        }

        [PermissionAuthorize("customer.view")]
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (customer == null) return NotFound();

            var sales = _context.Sales.AsNoTracking()
                .Where(s => s.CustomerId == id && s.Status != Sale.CancelledStatus);
            var lastSale = await sales.OrderByDescending(s => s.SaleDate).FirstOrDefaultAsync();

            return View(new CustomerDetailViewModel
            {
                Customer = customer,
                TotalShopping = await sales.SumAsync(s => (decimal?)s.TotalAmount) ?? 0,
                TotalOrders = await sales.CountAsync(),
                LastSaleDate = lastSale?.SaleDate,
                LastSaleAmount = lastSale?.TotalAmount
            });
        }

        [PermissionAuthorize("customer.update")]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            return customer == null ? NotFound() : View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("customer.update")]
        public async Task<IActionResult> Edit(Customer model)
        {
            var customer = await _context.Customers.FindAsync(model.Id);
            if (customer == null) return NotFound();
            if (string.IsNullOrWhiteSpace(model.FullName)) ModelState.AddModelError(nameof(model.FullName), "Müşteri adı zorunludur.");
            if (!ModelState.IsValid) return View(model);

            customer.FullName = model.FullName.Trim();
            customer.Phone = model.Phone?.Trim();
            customer.Email = model.Email?.Trim();
            customer.Address = model.Address?.Trim();
            customer.DiscountRate = Math.Clamp(model.DiscountRate, 0, 100);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Müşteri bilgileri güncellendi.";
            return RedirectToAction(nameof(Details), new { id = customer.Id });
        }

        private static string NormalizeStatusFilter(string? status)
        {
            return status is "all" or "inactive" ? status : "active";
        }
    }
}
