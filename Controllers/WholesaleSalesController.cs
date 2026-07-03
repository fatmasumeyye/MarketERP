using MarketERP.Data;
using MarketERP.Helpers;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarketERP.Services;
using System.Data;

namespace MarketERP.Controllers
{
    [PermissionAuthorize("sale.view.all", "sale.view.branch", "role.manage")]
    public class WholesaleSalesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IStockMovementService _stockMovementService;

        public WholesaleSalesController(
            AppDbContext context,
            IStockMovementService stockMovementService)
        {
            _context = context;
            _stockMovementService = stockMovementService;
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? reviewNote)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

            try
            {
                var request = await _context.WholesaleSaleRequests
                    .FromSqlInterpolated(
                        $"SELECT * FROM wholesale_sale_requests WHERE id = {id} FOR UPDATE")
                    .SingleOrDefaultAsync();

                if (request == null)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Toptan satış talebi bulunamadı.";
                    return RedirectToAction("Index");
                }

                if (request.Status != "Onay Bekliyor" || request.SaleId.HasValue)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Sadece daha önce satışa dönüşmemiş onay bekleyen talepler onaylanabilir.";
                    return RedirectToAction("Index");
                }

                await _context.Entry(request)
                    .Collection(r => r.Items)
                    .Query()
                    .Include(i => i.Product)
                    .LoadAsync();

                foreach (var item in request.Items)
                {
                    if (item.Product == null)
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = "Talepte ürün bilgisi eksik.";
                        return RedirectToAction("Index");
                    }

                    if (item.Product.StockQuantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();
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
                await _context.SaveChangesAsync();

                var saleDetails = request.Items.Select(item => new SaleDetail
                    {
                        SaleId = sale.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Subtotal = item.Subtotal
                    })
                    .ToList();

                _context.SaleDetails.AddRange(saleDetails);
                await _context.SaveChangesAsync();

                string saleNo = $"SAT-{sale.Id:D6}";
                foreach (var detail in saleDetails)
                {
                    await _stockMovementService.RecordAsync(new StockMovementCommand(
                        ProductId: detail.ProductId,
                        MovementType: StockMovementService.OutboundMovement,
                        ReasonType: "Satis",
                        Quantity: detail.Quantity,
                        MovementDate: sale.SaleDate,
                        SourceType: "Sale",
                        SourceId: sale.Id,
                        SourceLineId: detail.Id,
                        SourceNo: saleNo,
                        Description: "Toptan satış stok çıkışı.",
                        CreatedByEmployeeId: HttpContext.Session.GetInt32("EmployeeId")));
                }

                request.Status = "Onaylandı";
                request.ApprovedAt = DateTime.Now;
                request.ReviewNote = reviewNote;
                request.SaleId = sale.Id;

                _context.FinansHareketleri.Add(new FinansHareketi
                {
                    Tip = "Gelir",
                    Kategori = "Satis",
                    Baslik = "Toptan Satış",
                    Tutar = sale.TotalAmount,
                    Tarih = sale.SaleDate,
                    Durum = "Bekliyor",
                    OdemeYontemi = "BankaHavalesi",
                    Aciklama = $"{saleNo} numaralı toptan satış için tahsilat bekleniyor.",
                    OlusturanKullaniciId = HttpContext.Session.GetInt32("EmployeeId"),
                    OlusturmaTarihi = DateTime.Now,
                    KaynakTipi = "Sale",
                    KaynakId = sale.Id,
                    KaynakNo = saleNo,
                    OtomatikMi = true
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Toptan satış talebi onaylandı; satış, stok ve bekleyen finans hareketi birlikte kaydedildi.";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Toptan satış onaylanamadı. Satış, stok ve finans kayıtlarında değişiklik yapılmadı.";
            }

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
