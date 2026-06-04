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
        public IActionResult Index(string search)
        {
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

            ViewBag.TotalSalesRevenue = _context.Sales.Sum(s => s.TotalAmount);
            ViewBag.TotalSalesCount = _context.Sales.Count();

            return View(sales.OrderByDescending(s => s.SaleDate).ToList());
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
            return View();
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