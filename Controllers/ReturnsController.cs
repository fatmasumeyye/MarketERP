using MarketERP.Data;
using MarketERP.Helpers;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketERP.Controllers
{
    public class ReturnsController : Controller
    {
        private readonly AppDbContext _context;

        public ReturnsController(AppDbContext context)
        {
            _context = context;
        }

        [PermissionAuthorize("return.request")]
        public IActionResult MyRequests()
        {
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (employeeId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var requests = _context.ReturnRequests
                .Include(r => r.Sale)
                .Include(r => r.Product)
                .Where(r => r.EmployeeId == employeeId.Value)
                .OrderByDescending(r => r.RequestedAt)
                .ToList();

            return View(requests);
        }

        [PermissionAuthorize("return.request")]
        public IActionResult Create(int id)
        {
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (employeeId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var sale = _context.Sales
                .Include(s => s.Customer)
                .FirstOrDefault(s => s.Id == id && s.EmployeeId == employeeId.Value);

            if (sale == null)
            {
                return RedirectToAction("AccessDenied", "Login");
            }

            var details = _context.SaleDetails
                .Include(d => d.Product)
                .Where(d => d.SaleId == sale.Id)
                .ToList();

            ViewBag.Sale = sale;
            ViewBag.Details = details;

            return View();
        }

        [HttpPost]
        [PermissionAuthorize("return.request")]
        public IActionResult Create(int saleId, int saleDetailId, int quantity, string reason)
        {
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (employeeId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var sale = _context.Sales
                .FirstOrDefault(s => s.Id == saleId && s.EmployeeId == employeeId.Value);

            if (sale == null)
            {
                return RedirectToAction("AccessDenied", "Login");
            }

            var detail = _context.SaleDetails
                .Include(d => d.Product)
                .FirstOrDefault(d => d.Id == saleDetailId && d.SaleId == saleId);

            if (detail == null)
            {
                TempData["Error"] = "Satış detayı bulunamadı.";
                return RedirectToAction("Create", new { id = saleId });
            }

            if (quantity <= 0 || quantity > detail.Quantity)
            {
                TempData["Error"] = "İade adedi geçersiz.";
                return RedirectToAction("Create", new { id = saleId });
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "İade sebebi boş olamaz.";
                return RedirectToAction("Create", new { id = saleId });
            }

            var request = new ReturnRequest
            {
                SaleId = saleId,
                SaleDetailId = saleDetailId,
                ProductId = detail.ProductId,
                EmployeeId = employeeId.Value,
                Quantity = quantity,
                Reason = reason,
                Status = "Beklemede",
                RequestedAt = DateTime.Now
            };

            _context.ReturnRequests.Add(request);
            _context.SaveChanges();

            TempData["Success"] = "İade talebi oluşturuldu. Onay için mağaza müdürüne/admin'e gönderildi.";

            return RedirectToAction("MyRequests");
        }
    }
}