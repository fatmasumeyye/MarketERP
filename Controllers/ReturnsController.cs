using MarketERP.Data;
using MarketERP.Helpers;
using MarketERP.Models;
using MarketERP.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace MarketERP.Controllers
{
    public class ReturnsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IStockMovementService _stockMovementService;

        public ReturnsController(
            AppDbContext context,
            IStockMovementService stockMovementService)
        {
            _context = context;
            _stockMovementService = stockMovementService;
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

            if (sale.Status == Sale.CancelledStatus)
            {
                TempData["Error"] = "İptal edilmiş satış için iade talebi oluşturulamaz.";
                return RedirectToAction("MySales", "Sales");
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
        [ValidateAntiForgeryToken]
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

            if (sale.Status == Sale.CancelledStatus)
            {
                TempData["Error"] = "İptal edilmiş satış için iade talebi oluşturulamaz.";
                return RedirectToAction("MySales", "Sales");
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
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("sale.view.all", "sale.view.branch", "role.manage")]
        public async Task<IActionResult> ApproveDocument(string requestNo, string? reviewNote, IFormCollection form)
        {
            if (string.IsNullOrWhiteSpace(requestNo))
            {
                TempData["Error"] = "İade belge numarası bulunamadı.";
                return RedirectToAction("Index");
            }

            requestNo = requestNo.Trim();

            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

            try
            {
                const string pendingStatus = "Beklemede";
                var requests = await _context.ReturnRequests
                    .FromSqlInterpolated(
                        $"SELECT * FROM return_requests WHERE request_no = {requestNo} AND status = {pendingStatus} ORDER BY id FOR UPDATE")
                    .ToListAsync();

                if (!requests.Any())
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Onaylanacak bekleyen iade belgesi bulunamadı.";
                    return RedirectToAction("Index");
                }

                var requestIds = requests.Select(r => r.Id).ToList();
                await _context.ReturnRequests
                    .Where(r => requestIds.Contains(r.Id))
                    .Include(r => r.Product)
                    .Include(r => r.SaleDetail)
                    .Include(r => r.Sale)
                    .LoadAsync();

                if (requests.Any(r => r.Sale == null || r.Sale.Status == Sale.CancelledStatus))
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "İptal edilmiş satışa bağlı iade belgesi onaylanamaz.";
                    return RedirectToAction("Index");
                }

                if (requests.Any(r => r.SaleDetail == null || r.SaleDetail.Quantity <= 0))
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "İade satırlarından birinin satış detayı bulunamadı.";
                    return RedirectToAction("Index");
                }

                int saleId = requests[0].SaleId;
                if (requests.Any(r => r.SaleId != saleId))
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "İade belgesinde birden fazla satışa ait satır bulunduğu için işlem tamamlanamadı.";
                    return RedirectToAction("Index");
                }

                foreach (var request in requests)
                {
                    string stockActionKey = $"stockAction_{request.Id}";
                    string stockAction = form[stockActionKey].ToString();

                    if (stockAction != "Satılabilir stoğa ekle"
                        && stockAction != "Hasarlı / fire, stoğa ekleme")
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = $"{request.Product?.Name} için geçerli bir stok işlemi seçmelisiniz.";
                        return RedirectToAction("Index");
                    }
                }

                var reviewedAt = DateTime.Now;
                decimal originalLinesTotal = await _context.SaleDetails
                    .Where(d => d.SaleId == saleId)
                    .SumAsync(d => (decimal?)d.Subtotal) ?? 0m;

                if (originalLinesTotal <= 0)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Satışın finansal toplamı doğrulanamadığı için iade onaylanamadı.";
                    return RedirectToAction("Index");
                }

                decimal netSaleFactor = requests[0].Sale!.TotalAmount / originalLinesTotal;
                decimal returnTotal = Math.Round(
                    requests.Sum(r =>
                        (r.SaleDetail!.Subtotal * r.Quantity / r.SaleDetail.Quantity) * netSaleFactor),
                    2,
                    MidpointRounding.AwayFromZero);
                int sourceId = requests[0].Id;

                foreach (var request in requests)
                {
                    string stockAction = form[$"stockAction_{request.Id}"].ToString();

                    if (stockAction == "Satılabilir stoğa ekle")
                    {
                        await _stockMovementService.RecordAsync(new StockMovementCommand(
                            ProductId: request.ProductId,
                            MovementType: StockMovementService.InboundMovement,
                            ReasonType: "Iade",
                            Quantity: request.Quantity,
                            MovementDate: reviewedAt,
                            SourceType: "Return",
                            SourceId: sourceId,
                            SourceLineId: request.Id,
                            SourceNo: requestNo,
                            Description: "Satılabilir satış iadesi stok girişi.",
                            CreatedByEmployeeId: HttpContext.Session.GetInt32("EmployeeId"),
                            AllowInactiveProduct: true));
                    }

                    request.Status = "Onaylandı";
                    request.ReviewedAt = reviewedAt;

                    request.ReviewNote =
                        $"Stok işlemi: {stockAction}" +
                        (string.IsNullOrWhiteSpace(reviewNote) ? "" : $" | Yönetici notu: {reviewNote.Trim()}");
                }

                string paymentMethod = requests[0].Sale!.PaymentType switch
                {
                    "Nakit" => "Nakit",
                    "Kart" => "POS",
                    "Toptan" => "BankaHavalesi",
                    _ => "Diger"
                };

                _context.FinansHareketleri.Add(new FinansHareketi
                {
                    Tip = "Gider",
                    Kategori = "Iade",
                    Baslik = "Satış İadesi",
                    Tutar = returnTotal,
                    Tarih = reviewedAt,
                    Durum = "Odendi",
                    OdemeYontemi = paymentMethod,
                    Aciklama = $"{requestNo} numaralı satış iadesi.",
                    OlusturanKullaniciId = HttpContext.Session.GetInt32("EmployeeId"),
                    OlusturmaTarihi = reviewedAt,
                    KaynakTipi = "Return",
                    KaynakId = sourceId,
                    KaynakNo = requestNo,
                    OtomatikMi = true
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"{requestNo} numaralı iade belgesi onaylandı; stok ve finans işlemleri birlikte kaydedildi.";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "İade onaylanamadı. İade, stok ve finans kayıtlarında değişiklik yapılmadı.";
            }

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
