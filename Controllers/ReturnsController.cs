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

        [PermissionAuthorize("sale.view.all", "sale.view.branch", "role.manage")]
        public IActionResult Index(string status)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var requests = _context.ReturnRequests
                .Include(r => r.Sale)
                    .ThenInclude(s => s.Customer)
                .Include(r => r.SaleDetail)
                .Include(r => r.Product)
                .Include(r => r.Employee)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                requests = requests.Where(r => r.Status == status);
            }

            ViewBag.Status = status;

            ViewBag.TotalCount = _context.ReturnRequests
                .Select(r => r.RequestNo)
                .Distinct()
                .Count();

            ViewBag.PendingCount = _context.ReturnRequests
                .Where(r => r.Status == "Beklemede")
                .Select(r => r.RequestNo)
                .Distinct()
                .Count();

            ViewBag.ApprovedCount = _context.ReturnRequests
                .Where(r => r.Status == "Onaylandı")
                .Select(r => r.RequestNo)
                .Distinct()
                .Count();

            ViewBag.RejectedCount = _context.ReturnRequests
                .Where(r => r.Status == "Reddedildi")
                .Select(r => r.RequestNo)
                .Distinct()
                .Count();

            return View(requests.OrderByDescending(r => r.RequestedAt).ToList());
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
                .Include(r => r.SaleDetail)
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
                .Include(s => s.Employee)
                .FirstOrDefault(s => s.Id == id && s.EmployeeId == employeeId.Value);

            if (sale == null)
            {
                return RedirectToAction("AccessDenied", "Login");
            }

            var details = _context.SaleDetails
                .Include(d => d.Product)
                .Where(d => d.SaleId == sale.Id)
                .ToList();

            var availableQuantities = new Dictionary<int, int>();

            foreach (var detail in details)
            {
                int alreadyRequestedQuantity = _context.ReturnRequests
                    .Where(r =>
                        r.SaleDetailId == detail.Id &&
                        r.Status != "Reddedildi")
                    .Sum(r => (int?)r.Quantity) ?? 0;

                int availableQuantity = detail.Quantity - alreadyRequestedQuantity;

                if (availableQuantity < 0)
                {
                    availableQuantity = 0;
                }

                availableQuantities[detail.Id] = availableQuantity;
            }

            ViewBag.Sale = sale;
            ViewBag.Details = details;
            ViewBag.AvailableQuantities = availableQuantities;

            return View();
        }

        [HttpPost]
        [PermissionAuthorize("return.request")]
        public IActionResult Create(int saleId, List<int> selectedSaleDetailIds, IFormCollection form)
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

            if (selectedSaleDetailIds == null || selectedSaleDetailIds.Count == 0)
            {
                TempData["Error"] = "İade edilecek en az bir ürün seçmelisiniz.";
                return RedirectToAction("Create", new { id = saleId });
            }

            var createdRequests = new List<ReturnRequest>();

            foreach (var saleDetailId in selectedSaleDetailIds)
            {
                var detail = _context.SaleDetails
                    .Include(d => d.Product)
                    .FirstOrDefault(d => d.Id == saleDetailId && d.SaleId == saleId);

                if (detail == null)
                {
                    continue;
                }

                string quantityKey = $"quantity_{saleDetailId}";
                string reasonTypeKey = $"reasonType_{saleDetailId}";
                string reasonKey = $"reason_{saleDetailId}";

                if (!int.TryParse(form[quantityKey], out int quantity))
                {
                    continue;
                }

                string reasonType = form[reasonTypeKey].ToString();
                string reason = form[reasonKey].ToString();

                if (string.IsNullOrWhiteSpace(reasonType))
                {
                    TempData["Error"] = $"{detail.Product?.Name} için iade sebebi seçmelisiniz.";
                    return RedirectToAction("Create", new { id = saleId });
                }

                if (string.IsNullOrWhiteSpace(reason))
                {
                    TempData["Error"] = $"{detail.Product?.Name} için açıklama yazmalısınız.";
                    return RedirectToAction("Create", new { id = saleId });
                }

                int alreadyRequestedQuantity = _context.ReturnRequests
                    .Where(r =>
                        r.SaleDetailId == saleDetailId &&
                        r.Status != "Reddedildi")
                    .Sum(r => (int?)r.Quantity) ?? 0;

                int availableQuantity = detail.Quantity - alreadyRequestedQuantity;

                if (quantity <= 0 || quantity > availableQuantity)
                {
                    TempData["Error"] = $"{detail.Product?.Name} için iade adedi geçersiz. İade edilebilir kalan adet: {availableQuantity}";
                    return RedirectToAction("Create", new { id = saleId });
                }

                var request = new ReturnRequest
                {
                    RequestNo = "",
                    SaleId = saleId,
                    SaleDetailId = saleDetailId,
                    ProductId = detail.ProductId,
                    EmployeeId = employeeId.Value,
                    Quantity = quantity,
                    ReasonType = reasonType.Trim(),
                    Reason = reason.Trim(),
                    Status = "Beklemede",
                    RequestedAt = DateTime.Now
                };

                _context.ReturnRequests.Add(request);
                createdRequests.Add(request);
            }

            if (!createdRequests.Any())
            {
                TempData["Error"] = "Geçerli ürün seçimi bulunamadı.";
                return RedirectToAction("Create", new { id = saleId });
            }

            _context.SaveChanges();

            string documentNo = $"RT-{createdRequests.First().Id:D6}";

            foreach (var request in createdRequests)
            {
                request.RequestNo = documentNo;
            }

            _context.SaveChanges();

            TempData["Success"] = $"{documentNo} numaralı iade talep belgesi oluşturuldu. Seçilen {createdRequests.Count} ürün aynı belgeye bağlandı.";

            return RedirectToAction("MyRequests");
        }

        [PermissionAuthorize("return.request", "sale.view.all", "sale.view.branch", "role.manage")]
        public IActionResult ReturnInvoice(int id)
        {
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (employeeId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var firstRequest = _context.ReturnRequests
                .FirstOrDefault(r => r.Id == id);

            if (firstRequest == null)
            {
                return NotFound();
            }

            string requestNo = string.IsNullOrWhiteSpace(firstRequest.RequestNo)
                ? $"RT-{firstRequest.Id:D6}"
                : firstRequest.RequestNo;

            var requests = _context.ReturnRequests
                .Include(r => r.Sale)
                    .ThenInclude(s => s.Customer)
                .Include(r => r.Sale)
                    .ThenInclude(s => s.Employee)
                .Include(r => r.SaleDetail)
                .Include(r => r.Product)
                .Include(r => r.Employee)
                .Where(r => r.RequestNo == requestNo)
                .OrderBy(r => r.Id)
                .ToList();

            if (!requests.Any())
            {
                return NotFound();
            }

            var isAdminOrManager =
                HttpContext.HasPermission("sale.view.all")
                || HttpContext.HasPermission("sale.view.branch")
                || HttpContext.HasPermission("role.manage");

            if (!isAdminOrManager && requests.Any(r => r.EmployeeId != employeeId.Value))
            {
                return RedirectToAction("AccessDenied", "Login");
            }

            ViewBag.BackUrl = isAdminOrManager ? "/Returns" : "/Sales/MySales";
            ViewBag.DocumentNo = requestNo;

            return View(requests);
        }

        [HttpPost]
        [PermissionAuthorize("sale.view.all", "sale.view.branch", "role.manage")]
        public IActionResult ApproveDocument(string requestNo, string? reviewNote, IFormCollection form)
        {
            if (string.IsNullOrWhiteSpace(requestNo))
            {
                TempData["Error"] = "İade belge numarası bulunamadı.";
                return RedirectToAction("Index");
            }

            var requests = _context.ReturnRequests
                .Include(r => r.Product)
                .Where(r => r.RequestNo == requestNo && r.Status == "Beklemede")
                .ToList();

            if (!requests.Any())
            {
                TempData["Error"] = "Onaylanacak bekleyen iade belgesi bulunamadı.";
                return RedirectToAction("Index");
            }

            foreach (var request in requests)
            {
                string stockActionKey = $"stockAction_{request.Id}";
                string stockAction = form[stockActionKey].ToString();

                if (string.IsNullOrWhiteSpace(stockAction))
                {
                    TempData["Error"] = $"{request.Product?.Name} için stok işlemi seçmelisiniz.";
                    return RedirectToAction("Index");
                }

                bool addToStock = stockAction == "Satılabilir stoğa ekle";

                if (addToStock && request.Product != null)
                {
                    request.Product.StockQuantity += request.Quantity;
                }

                request.Status = "Onaylandı";
                request.ReviewedAt = DateTime.Now;

                request.ReviewNote =
                    $"Stok işlemi: {stockAction}" +
                    (string.IsNullOrWhiteSpace(reviewNote) ? "" : $" | Yönetici notu: {reviewNote}");
            }

            _context.SaveChanges();

            TempData["Success"] = $"{requestNo} numaralı iade belgesi onaylandı. Ürün satırlarına göre stok işlemleri uygulandı.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [PermissionAuthorize("sale.view.all", "sale.view.branch", "role.manage")]
        public IActionResult RejectDocument(string requestNo, string? reviewNote)
        {
            if (string.IsNullOrWhiteSpace(requestNo))
            {
                TempData["Error"] = "İade belge numarası bulunamadı.";
                return RedirectToAction("Index");
            }

            var requests = _context.ReturnRequests
                .Where(r => r.RequestNo == requestNo && r.Status == "Beklemede")
                .ToList();

            if (!requests.Any())
            {
                TempData["Error"] = "Reddedilecek bekleyen iade belgesi bulunamadı.";
                return RedirectToAction("Index");
            }

            foreach (var request in requests)
            {
                request.Status = "Reddedildi";
                request.ReviewedAt = DateTime.Now;
                request.ReviewNote = reviewNote;
            }

            _context.SaveChanges();

            TempData["Success"] = $"{requestNo} numaralı iade belgesi reddedildi.";

            return RedirectToAction("Index");
        }
    }
}