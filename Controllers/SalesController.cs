using MarketERP.Data;
using MarketERP.Helpers;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarketERP.Services;
using System.Data;
using System.Text.Json;

namespace MarketERP.Controllers
{
    public class SalesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IStockMovementService _stockMovementService;

        public SalesController(
            AppDbContext context,
            IStockMovementService stockMovementService)
        {
            _context = context;
            _stockMovementService = stockMovementService;
        }

        [PermissionAuthorize("sale.view.all", "sale.view.branch")]
        public IActionResult Index(string search, string dateFilter)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            DateTime weekStart = today.AddDays(-((int)today.DayOfWeek == 0 ? 6 : (int)today.DayOfWeek - 1));
            DateTime monthStart = new DateTime(today.Year, today.Month, 1);
            DateTime yearStart = new DateTime(today.Year, 1, 1);

            var sales = _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Employee)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                sales = sales.Where(s =>
                    s.Customer != null &&
                    s.Customer.FullName.Contains(search));
            }

            if (dateFilter == "today")
            {
                sales = sales.Where(s => s.SaleDate >= today && s.SaleDate < tomorrow);
                ViewBag.FilterTitle = "Bugünkü Satışlar";
            }
            else if (dateFilter == "week")
            {
                sales = sales.Where(s => s.SaleDate >= weekStart && s.SaleDate < tomorrow);
                ViewBag.FilterTitle = "Bu Haftaki Satışlar";
            }
            else if (dateFilter == "month")
            {
                sales = sales.Where(s => s.SaleDate >= monthStart && s.SaleDate < tomorrow);
                ViewBag.FilterTitle = "Bu Aydaki Satışlar";
            }
            else if (dateFilter == "year")
            {
                sales = sales.Where(s => s.SaleDate >= yearStart && s.SaleDate < tomorrow);
                ViewBag.FilterTitle = "Bu Yıldaki Satışlar";
            }
            else
            {
                ViewBag.FilterTitle = "Tüm Satışlar";
            }

            var filteredSales = sales
                .OrderByDescending(s => s.SaleDate)
                .ToList();

            ViewBag.DateFilter = dateFilter;
            ViewBag.Search = search;
            var activeSales = filteredSales
                .Where(s => s.Status != Sale.CancelledStatus)
                .ToList();

            ViewBag.TotalSalesRevenue = activeSales.Sum(s => s.TotalAmount);
            ViewBag.TotalSalesCount = activeSales.Count;
            ViewBag.CancelledSalesCount = filteredSales.Count - activeSales.Count;

            return View(filteredSales);
        }

        [PermissionAuthorize("sale.retail.create")]
        public IActionResult Retail()
        {
            ViewBag.ProductList = _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToList();

            ViewBag.Customers = new SelectList(
                _context.Customers
                    .Where(c => c.IsActive && c.FullName != "Nihai Tüketici")
                    .OrderBy(c => c.FullName)
                    .ToList(),
                "Id",
                "FullName"
            );

            var cart = GetCart();

            ViewBag.Cart = cart;
            ViewBag.CartTotal = cart.Sum(x => x.Subtotal);

            return View(cart);
        }

        [PermissionAuthorize("sale.view.own")]
        public IActionResult MySales()
        {
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (employeeId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!_context.Employees.Any(e => e.Id == employeeId.Value && e.IsActive))
            {
                HttpContext.Session.Clear();
                TempData["Error"] = "Çalışan kaydınız pasif olduğu için yeni işlem oluşturamazsınız.";
                return RedirectToAction("Index", "Login");
            }

            var sales = _context.Sales
                .ActiveSales()
                .Include(s => s.Customer)
                .Where(s => s.EmployeeId == employeeId.Value)
                .OrderByDescending(s => s.SaleDate)
                .ToList();

            return View(sales);
        }

        [PermissionAuthorize("sale.wholesale.create")]
        public IActionResult Wholesale()
        {
            var customers = _context.Customers
                .Where(c => c.IsActive && c.FullName != "Nihai Tüketici")
                .OrderBy(c => c.FullName)
                .ToList();

            ViewBag.Customers = new SelectList(
                customers,
                "Id",
                "FullName"
            );

            ViewBag.CustomersJson = JsonSerializer.Serialize(
                customers.Select(c => new
                {
                    id = c.Id,
                    name = c.FullName,
                    phone = c.Phone,
                    email = c.Email,
                    address = c.Address,
                    discountRate = c.DiscountRate
                })
            );

            ViewBag.Products = _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToList();

            return View();
        }

        [HttpPost]
        [PermissionAuthorize("sale.wholesale.create")]
        public IActionResult Wholesale(
            int customerId,
            DateTime? dueDate,
            DateTime? deliveryDate,
            DateTime? offerValidUntil,
            string? deliveryAddress,
            string? paymentType,
            string? note,
            List<int> productIds,
            List<int> quantities)
        {
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (employeeId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (customerId <= 0)
            {
                TempData["Error"] = "Toptan satış talebi için müşteri/firma seçmelisiniz.";
                return RedirectToAction("Wholesale");
            }

            var customer = _context.Customers.FirstOrDefault(c => c.Id == customerId && c.IsActive);

            if (customer == null)
            {
                TempData["Error"] = "Seçilen müşteri/firma bulunamadı.";
                return RedirectToAction("Wholesale");
            }

            if (string.IsNullOrWhiteSpace(paymentType))
            {
                TempData["Error"] = "Ödeme tipi seçmelisiniz.";
                return RedirectToAction("Wholesale");
            }

            if (productIds == null || quantities == null || productIds.Count == 0 || quantities.Count == 0)
            {
                TempData["Error"] = "Toptan satış talebi için en az bir ürün seçmelisiniz.";
                return RedirectToAction("Wholesale");
            }

            var items = new List<WholesaleSaleRequestItem>();

            for (int i = 0; i < productIds.Count; i++)
            {
                int productId = productIds[i];
                int quantity = quantities.Count > i ? quantities[i] : 0;

                if (productId <= 0 || quantity <= 0)
                {
                    continue;
                }

                var product = _context.Products.FirstOrDefault(p => p.Id == productId && p.IsActive);

                if (product == null)
                {
                    continue;
                }

                if (product.StockQuantity < quantity)
                {
                    TempData["Error"] = $"{product.Name} için stok yetersiz. Mevcut stok: {product.StockQuantity}";
                    return RedirectToAction("Wholesale");
                }

                items.Add(new WholesaleSaleRequestItem
                {
                    ProductId = product.Id,
                    Quantity = quantity,
                    UnitPrice = product.SalePrice,
                    Subtotal = product.SalePrice * quantity
                });
            }

            if (!items.Any())
            {
                TempData["Error"] = "Geçerli ürün ve miktar bilgisi girilmedi.";
                return RedirectToAction("Wholesale");
            }

            decimal subtotal = items.Sum(i => i.Subtotal);

            decimal discountRate = customer.DiscountRate;

            if (discountRate < 0)
            {
                discountRate = 0;
            }

            if (discountRate > 100)
            {
                discountRate = 100;
            }

            decimal discountAmount = subtotal * discountRate / 100;
            decimal totalAmount = subtotal - discountAmount;

            var request = new WholesaleSaleRequest
            {
                CustomerId = customerId,
                EmployeeId = employeeId.Value,
                RequestDate = DateTime.Now,
                DueDate = dueDate,
                DeliveryDate = deliveryDate,
                OfferValidUntil = offerValidUntil,
                DeliveryAddress = deliveryAddress,
                PaymentType = paymentType,
                DiscountRate = discountRate,
                SubtotalAmount = subtotal,
                DiscountAmount = discountAmount,
                TotalAmount = totalAmount,
                Status = "Onay Bekliyor",
                Note = note,
                Items = items
            };

            _context.WholesaleSaleRequests.Add(request);
            _context.SaveChanges();

            TempData["Success"] =
                $"Toptan satış talebi oluşturuldu. Müşteriye bağlı %{discountRate:0.##} iskonto uygulandı ve yönetici onayına gönderildi.";

            return RedirectToAction("Wholesale");
        }

        [HttpGet]
        public IActionResult AddToCart()
        {
            return RedirectToAction("Retail");
        }

        [HttpPost]
        [PermissionAuthorize("sale.retail.create")]
        public IActionResult AddToCart(int productId, int quantity)
        {
            return AddProductToCart(productId, quantity);
        }

        [HttpPost]
        [PermissionAuthorize("sale.retail.create")]
        public IActionResult AddBarcodeToCart(string barcode, int quantity = 1)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                TempData["Error"] = "Barkod alanı boş olamaz.";
                return RedirectToAction("Retail");
            }

            if (quantity <= 0)
            {
                TempData["Error"] = "Adet 0'dan büyük olmalıdır.";
                return RedirectToAction("Retail");
            }

            var product = _context.Products
                .FirstOrDefault(p => p.Barcode == barcode && p.IsActive);

            if (product == null)
            {
                TempData["Error"] = "Bu barkoda ait ürün bulunamadı.";
                return RedirectToAction("Retail");
            }

            return AddProductToCart(product.Id, quantity);
        }

        [PermissionAuthorize("sale.retail.create")]
        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }

            return RedirectToAction("Retail");
        }

        [PermissionAuthorize("sale.retail.create")]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Retail");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("sale.retail.create")]
        public async Task<IActionResult> CompleteSale(int? customerId, string paymentType)
        {
            var cart = GetCart();

            if (string.IsNullOrWhiteSpace(paymentType))
            {
                TempData["Error"] = "Ödeme tipi seçmelisiniz.";
                return RedirectToAction("Retail");
            }

            if (paymentType != "Nakit" && paymentType != "Kart")
            {
                TempData["Error"] = "Geçersiz ödeme tipi.";
                return RedirectToAction("Retail");
            }

            if (cart.Count == 0)
            {
                TempData["Error"] = "Sepet boş. Önce ürün ekleyin.";
                return RedirectToAction("Retail");
            }

            if (customerId.HasValue && !await _context.Customers
                    .AnyAsync(c => c.Id == customerId.Value && c.IsActive))
            {
                TempData["Error"] = "Seçilen müşteri aktif değil veya bulunamadı.";
                return RedirectToAction("Retail");
            }

            decimal total = cart.Sum(x => x.Subtotal);
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (!employeeId.HasValue || !await _context.Employees
                    .AnyAsync(e => e.Id == employeeId.Value && e.IsActive))
            {
                HttpContext.Session.Clear();
                TempData["Error"] = "Çalışan kaydınız pasif olduğu için yeni satış oluşturamazsınız.";
                return RedirectToAction("Index", "Login");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

            try
            {
                if (!customerId.HasValue)
                {
                    var defaultCustomer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.FullName == "Nihai Tüketici" && c.IsActive);

                    if (defaultCustomer == null)
                    {
                        defaultCustomer = new Customer
                        {
                            FullName = "Nihai Tüketici",
                            Phone = "-",
                            Email = "-",
                            Address = "-",
                            IsActive = true
                        };

                        _context.Customers.Add(defaultCustomer);
                        await _context.SaveChangesAsync();
                    }

                    customerId = defaultCustomer.Id;
                }

                var productIds = cart.Select(x => x.ProductId).Distinct().ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id) && p.IsActive)
                    .ToDictionaryAsync(p => p.Id);

                foreach (var item in cart)
                {
                    if (!products.TryGetValue(item.ProductId, out var product))
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = item.ProductName + " ürünü bulunamadı.";
                        return RedirectToAction("Retail");
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = item.ProductName + " için stok yetersiz.";
                        return RedirectToAction("Retail");
                    }
                }

                var sale = new Sale
                {
                    CustomerId = customerId,
                    EmployeeId = employeeId,
                    SaleDate = DateTime.Now,
                    TotalAmount = total,
                    PaymentType = paymentType
                };

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                var saleDetails = cart.Select(item => new SaleDetail
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
                        Description: "Perakende satış stok çıkışı.",
                        CreatedByEmployeeId: employeeId));
                }

                _context.FinansHareketleri.Add(new FinansHareketi
                {
                    Tip = "Gelir",
                    Kategori = "Satis",
                    Baslik = "Perakende Satış",
                    Tutar = sale.TotalAmount,
                    Tarih = sale.SaleDate,
                    Durum = "Odendi",
                    OdemeYontemi = paymentType == "Kart" ? "POS" : "Nakit",
                    Aciklama = $"{saleNo} numaralı perakende satış tahsilatı.",
                    OlusturanKullaniciId = employeeId,
                    OlusturmaTarihi = DateTime.Now,
                    KaynakTipi = "Sale",
                    KaynakId = sale.Id,
                    KaynakNo = saleNo,
                    OtomatikMi = true
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                HttpContext.Session.Remove("Cart");
                TempData["Success"] = "Perakende satış başarıyla tamamlandı ve finans hareketi oluşturuldu.";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Satış tamamlanamadı. Satış, stok ve finans kayıtlarında değişiklik yapılmadı.";
            }

            return RedirectToAction("Retail");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("sale.cancel")]
        public async Task<IActionResult> Cancel(int id, string? cancellationReason)
        {
            cancellationReason = cancellationReason?.Trim();

            if (string.IsNullOrWhiteSpace(cancellationReason))
            {
                TempData["Error"] = "Satış iptal nedeni zorunludur.";
                return RedirectToAction(nameof(Index));
            }

            if (cancellationReason.Length > 500)
            {
                TempData["Error"] = "Satış iptal nedeni en fazla 500 karakter olabilir.";
                return RedirectToAction(nameof(Index));
            }

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (!employeeId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

            try
            {
                var sale = await _context.Sales
                    .FromSqlInterpolated($"SELECT * FROM sales WHERE id = {id} FOR UPDATE")
                    .SingleOrDefaultAsync();

                if (sale == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound();
                }

                if (sale.Status == Sale.CancelledStatus)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Bu satış daha önce iptal edilmiş.";
                    return RedirectToAction(nameof(Index));
                }

                var details = await _context.SaleDetails
                    .Include(d => d.Product)
                    .Where(d => d.SaleId == sale.Id)
                    .ToListAsync();

                if (details.Any(d => d.Product == null))
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Satış kalemlerinden birinin ürün kaydı bulunamadığı için iptal tamamlanamadı.";
                    return RedirectToAction(nameof(Index));
                }

                string cancellationSourceNo = $"SAT-{sale.Id:D6}";
                foreach (var detail in details)
                {
                    int? originalMovementId = await _context.StockMovements
                        .Where(m => m.SourceType == "Sale"
                            && m.SourceId == sale.Id
                            && m.SourceLineId == detail.Id)
                        .Select(m => (int?)m.Id)
                        .FirstOrDefaultAsync();

                    await _stockMovementService.RecordAsync(new StockMovementCommand(
                        ProductId: detail.ProductId,
                        MovementType: StockMovementService.InboundMovement,
                        ReasonType: "SatisIptali",
                        Quantity: detail.Quantity,
                        MovementDate: DateTime.Now,
                        SourceType: "SaleCancellation",
                        SourceId: sale.Id,
                        SourceLineId: detail.Id,
                        SourceNo: cancellationSourceNo,
                        Description: $"Satış iptali: {cancellationReason}",
                        CreatedByEmployeeId: employeeId.Value,
                        ReversalOfMovementId: originalMovementId,
                        AllowInactiveProduct: true));
                }

                sale.Status = Sale.CancelledStatus;
                sale.CancellationReason = cancellationReason;
                sale.CancelledAt = DateTime.Now;
                sale.CancelledByEmployeeId = employeeId.Value;

                var automaticFinanceMovement = await _context.FinansHareketleri
                    .FirstOrDefaultAsync(h =>
                        h.OtomatikMi
                        && h.KaynakTipi == "Sale"
                        && h.KaynakId == sale.Id);

                if (automaticFinanceMovement != null)
                {
                    automaticFinanceMovement.Durum = "Iptal";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Satış iptal durumuna alındı ve ürün stokları geri eklendi.";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Satış iptal edilirken işlem tamamlanamadı. Hiçbir değişiklik kaydedilmedi.";
            }

            return RedirectToAction(nameof(Index));
        }

        [PermissionAuthorize(
            "sale.view.own",
            "sale.view.branch",
            "sale.view.all",
            "sale.retail.create",
            "sale.wholesale.create"
        )]

        [PermissionAuthorize(
    "sale.view.own",
    "sale.view.branch",
    "sale.view.all",
    "sale.retail.create",
    "sale.wholesale.create"
)]
        public IActionResult Invoice(int id, string? returnUrl = null)
        {
            var sale = _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Employee)
                .Include(s => s.CancelledByEmployee)
                .FirstOrDefault(s => s.Id == id);

            if (sale == null)
            {
                return NotFound();
            }

            var details = _context.SaleDetails
                .Include(d => d.Product)
                .Where(d => d.SaleId == sale.Id)
                .ToList();

            ViewBag.SaleDetails = details;
            ViewBag.ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/Sales/MySales" : returnUrl;

            return View(sale);
        }

        private IActionResult AddProductToCart(int productId, int quantity)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId && p.IsActive);

            if (product == null)
            {
                TempData["Error"] = "Ürün bulunamadı.";
                return RedirectToAction("Retail");
            }

            if (quantity <= 0)
            {
                TempData["Error"] = "Adet 0'dan büyük olmalıdır.";
                return RedirectToAction("Retail");
            }

            if (product.StockQuantity < quantity)
            {
                TempData["Error"] = "Yetersiz stok! Mevcut stok: " + product.StockQuantity;
                return RedirectToAction("Retail");
            }

            var cart = GetCart();

            var existingItem = cart.FirstOrDefault(x => x.ProductId == product.Id);

            if (existingItem != null)
            {
                if (product.StockQuantity < existingItem.Quantity + quantity)
                {
                    TempData["Error"] = "Yetersiz stok! Mevcut stok: " + product.StockQuantity;
                    return RedirectToAction("Retail");
                }

                existingItem.Quantity += quantity;
                existingItem.Subtotal = existingItem.Quantity * existingItem.UnitPrice;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = quantity,
                    UnitPrice = product.SalePrice,
                    Subtotal = quantity * product.SalePrice
                });
            }

            SaveCart(cart);

            return RedirectToAction("Retail");
        }

        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");

            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }

            return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("Cart", cartJson);
        }
    }

    public class CartItem
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Subtotal { get; set; }
    }
}
