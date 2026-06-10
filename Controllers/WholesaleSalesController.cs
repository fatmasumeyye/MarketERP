using MarketERP.Data;
using MarketERP.Helpers;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketERP.Controllers
{
    [PermissionAuthorize("sale.view.all", "sale.view.branch", "role.manage")]
    public class WholesaleSalesController : Controller
    {
        private readonly AppDbContext _context;

        public WholesaleSalesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string status)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var requests = _context.WholesaleSaleRequests
                .Include(r => r.Customer)
                .Include(r => r.Employee)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Product)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                requests = requests.Where(r => r.Status == status);
            }

            ViewBag.Status = status;

            ViewBag.PendingCount = _context.WholesaleSaleRequests.Count(r => r.Status == "Onay Bekliyor");
            ViewBag.ApprovedCount = _context.WholesaleSaleRequests.Count(r => r.Status == "Onaylandı");
            ViewBag.RejectedCount = _context.WholesaleSaleRequests.Count(r => r.Status == "Reddedildi");
            ViewBag.TotalCount = _context.WholesaleSaleRequests.Count();

            return View(requests.OrderByDescending(r => r.RequestDate).ToList());
        }

        [HttpPost]
        public IActionResult Approve(int id, string? reviewNote)
        {
            var request = _context.WholesaleSaleRequests
                .Include(r => r.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefault(r => r.Id == id);

            if (request == null)
            {
                TempData["Error"] = "Toptan satış talebi bulunamadı.";
                return RedirectToAction("Index");
            }

            if (request.Status != "Onay Bekliyor")
            {
                TempData["Error"] = "Sadece onay bekleyen talepler onaylanabilir.";
                return RedirectToAction("Index");
            }

            foreach (var item in request.Items)
            {
                if (item.Product == null)
                {
                    TempData["Error"] = "Talepte ürün bilgisi eksik.";
                    return RedirectToAction("Index");
                }

                if (item.Product.StockQuantity < item.Quantity)
                {
                    TempData["Error"] = $"{item.Product.Name} için stok yetersiz. Mevcut stok: {item.Product.StockQuantity}";
                    return RedirectToAction("Index");
                }
            }

            var sale = new Sale
            {
                CustomerId = request.CustomerId,
                EmployeeId = request.EmployeeId,
                SaleDate = DateTime.Now,
                TotalAmount = request.TotalAmount,
                PaymentType = "Toptan"
            };

            _context.Sales.Add(sale);
            _context.SaveChanges();

            foreach (var item in request.Items)
            {
                var detail = new SaleDetail
                {
                    SaleId = sale.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Subtotal
                };

                _context.SaleDetails.Add(detail);

                if (item.Product != null)
                {
                    item.Product.StockQuantity -= item.Quantity;
                }
            }

            request.Status = "Onaylandı";
            request.ApprovedAt = DateTime.Now;
            request.ReviewNote = reviewNote;
            request.SaleId = sale.Id;

            _context.SaveChanges();

            TempData["Success"] = "Toptan satış talebi onaylandı. Satış kaydı oluşturuldu ve stoklar düşüldü.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Reject(int id, string? reviewNote)
        {
            var request = _context.WholesaleSaleRequests.Find(id);

            if (request == null)
            {
                TempData["Error"] = "Toptan satış talebi bulunamadı.";
                return RedirectToAction("Index");
            }

            if (request.Status != "Onay Bekliyor")
            {
                TempData["Error"] = "Sadece onay bekleyen talepler reddedilebilir.";
                return RedirectToAction("Index");
            }

            request.Status = "Reddedildi";
            request.RejectedAt = DateTime.Now;
            request.ReviewNote = reviewNote;

            _context.SaveChanges();

            TempData["Success"] = "Toptan satış talebi reddedildi.";

            return RedirectToAction("Index");
        }
    }
}