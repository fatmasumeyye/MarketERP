using MarketERP.Data;
using MarketERP.Helpers;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MarketERP.Controllers
{
    public class SalesController : Controller
    {
        private readonly AppDbContext _context;

        public SalesController(AppDbContext context)
        {
            _context = context;
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
            ViewBag.TotalSalesRevenue = filteredSales.Sum(s => s.TotalAmount);
            ViewBag.TotalSalesCount = filteredSales.Count;

            return View(filteredSales);
        }

        [PermissionAuthorize("sale.retail.create")]
        public IActionResult Retail()
        {
            ViewBag.ProductList = _context.Products
                .OrderBy(p => p.Name)
                .ToList();

            ViewBag.Customers = new SelectList(
                _context.Customers
                    .Where(c => c.FullName != "Nihai Tüketici")
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

            var sales = _context.Sales
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
                .Where(c => c.FullName != "Nihai Tüketici")
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

            var customer = _context.Customers.FirstOrDefault(c => c.Id == customerId);

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

                var product = _context.Products.Find(productId);

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
                .FirstOrDefault(p => p.Barcode == barcode);

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
        [PermissionAuthorize("sale.retail.create")]
        public IActionResult CompleteSale(int? customerId, string paymentType)
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

            if (customerId == null)
            {
                var defaultCustomer = _context.Customers
                    .FirstOrDefault(c => c.FullName == "Nihai Tüketici");

                if (defaultCustomer == null)
                {
                    defaultCustomer = new Customer
                    {
                        FullName = "Nihai Tüketici",
                        Phone = "-",
                        Email = "-",
                        Address = "-"
                    };

                    _context.Customers.Add(defaultCustomer);
                    _context.SaveChanges();
                }

                customerId = defaultCustomer.Id;
            }

            foreach (var item in cart)
            {
                var product = _context.Products.Find(item.ProductId);

                if (product == null)
                {
                    TempData["Error"] = item.ProductName + " ürünü bulunamadı.";
                    return RedirectToAction("Retail");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    TempData["Error"] = item.ProductName + " için stok yetersiz.";
                    return RedirectToAction("Retail");
                }
            }

            decimal total = cart.Sum(x => x.Subtotal);
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            var sale = new Sale
            {
                CustomerId = customerId,
                EmployeeId = employeeId,
                SaleDate = DateTime.Now,
                TotalAmount = total,
                PaymentType = paymentType
            };

            _context.Sales.Add(sale);
            _context.SaveChanges();

            foreach (var item in cart)
            {
                var product = _context.Products.Find(item.ProductId);

                if (product == null)
                {
                    TempData["Error"] = item.ProductName + " ürünü bulunamadı.";
                    return RedirectToAction("Retail");
                }

                var detail = new SaleDetail
                {
                    SaleId = sale.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Subtotal
                };

                _context.SaleDetails.Add(detail);

                product.StockQuantity -= item.Quantity;
            }

            _context.SaveChanges();

            HttpContext.Session.Remove("Cart");

            TempData["Success"] = "Perakende satış başarıyla tamamlandı.";

            return RedirectToAction("Retail");
        }

        [PermissionAuthorize("sale.cancel")]
        public IActionResult Cancel(int id)
        {
            var sale = _context.Sales
                .Include(s => s.Customer)
                .FirstOrDefault(s => s.Id == id);

            if (sale == null)
            {
                return NotFound();
            }

            var details = _context.SaleDetails
                .Include(d => d.Product)
                .Where(d => d.SaleId == sale.Id)
                .ToList();

            foreach (var detail in details)
            {
                if (detail.Product != null)
                {
                    detail.Product.StockQuantity += detail.Quantity;
                }
            }

            _context.SaleDetails.RemoveRange(details);
            _context.Sales.Remove(sale);
            _context.SaveChanges();

            return RedirectToAction("Index");
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
            var product = _context.Products.Find(productId);

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