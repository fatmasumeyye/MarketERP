using MarketERP.Data;
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

        public IActionResult Index(string search)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Products = new SelectList(_context.Products.ToList(), "Id", "Name");
            ViewBag.ProductList = _context.Products.ToList();
            ViewBag.Customers = new SelectList(_context.Customers.ToList(), "Id", "FullName");

            var cart = GetCart();
            ViewBag.Cart = cart;
            ViewBag.CartTotal = cart.Sum(x => x.Subtotal);

            var sales = _context.SaleDetails
                .Include(s => s.Product)
                .Include(s => s.Sale)
                    .ThenInclude(s => s.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                sales = sales.Where(s => s.Product.Name.Contains(search));
            }

            ViewBag.TotalSalesRevenue = _context.Sales.Sum(s => s.TotalAmount);
            ViewBag.TotalSalesCount = _context.Sales.Count();

            ViewBag.SaleList = _context.Sales
    .Include(s => s.Customer)
    .OrderByDescending(s => s.SaleDate)
    .ToList();
            return View(sales.OrderByDescending(s => s.Id).ToList());
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity)
        {
            var product = _context.Products.Find(productId);

            if (product == null)
            {
                TempData["Error"] = "Ürün bulunamadı.";
                return RedirectToAction("Index");
            }

            if (quantity <= 0)
            {
                TempData["Error"] = "Adet 0'dan büyük olmalıdır.";
                return RedirectToAction("Index");
            }

            if (product.StockQuantity < quantity)
            {
                TempData["Error"] = "Yetersiz stok! Mevcut stok: " + product.StockQuantity;
                return RedirectToAction("Index");
            }

            var cart = GetCart();

            var existingItem = cart.FirstOrDefault(x => x.ProductId == product.Id);

            if (existingItem != null)
            {
                if (product.StockQuantity < existingItem.Quantity + quantity)
                {
                    TempData["Error"] = "Yetersiz stok! Mevcut stok: " + product.StockQuantity;
                    return RedirectToAction("Index");
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

            return RedirectToAction("Index");
        }

        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }

            return RedirectToAction("Index");
        }

        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CompleteSale(int? customerId)
        {
            var cart = GetCart();
            if (customerId == null)
            {
                TempData["Error"] = "Satışı tamamlamak için müşteri seçmelisiniz.";
                return RedirectToAction("Index");
            }

            if (cart.Count == 0)
            {
                TempData["Error"] = "Sepet boş. Önce ürün ekleyin.";
                return RedirectToAction("Index");
            }

            foreach (var item in cart)
            {
                var product = _context.Products.Find(item.ProductId);

                if (product == null)
                {
                    TempData["Error"] = item.ProductName + " ürünü bulunamadı.";
                    return RedirectToAction("Index");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    TempData["Error"] = item.ProductName + " için stok yetersiz.";
                    return RedirectToAction("Index");
                }
            }

            decimal total = cart.Sum(x => x.Subtotal);

            Sale sale = new Sale
            {
                CustomerId = customerId,
                SaleDate = DateTime.Now,
                TotalAmount = total
            };

            _context.Sales.Add(sale);
            _context.SaveChanges();

            foreach (var item in cart)
            {
                var product = _context.Products.Find(item.ProductId);

                SaleDetail detail = new SaleDetail
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

            return RedirectToAction("Index");
        }

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

        public IActionResult Invoice(int id)
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

            ViewBag.SaleDetails = details;

            return View(sale);
        }

        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");

            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }

            return JsonSerializer.Deserialize<List<CartItem>>(cartJson);
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

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Subtotal { get; set; }
    }
}