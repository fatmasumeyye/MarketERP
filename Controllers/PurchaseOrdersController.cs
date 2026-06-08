using MarketERP.Data;
using MarketERP.Helpers;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketERP.Controllers
{
    [PermissionAuthorize("purchase.create", "stock.view", "product.view")]
    public class PurchaseOrdersController : Controller
    {
        private readonly AppDbContext _context;

        public PurchaseOrdersController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string status)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var orders = _context.PurchaseOrders
                .Include(o => o.Supplier)
                .Include(o => o.Employee)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                orders = orders.Where(o => o.Status == status);
            }

            ViewBag.Status = status;

            return View(orders.OrderByDescending(o => o.OrderDate).ToList());
        }

        public IActionResult Details(int id)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var order = _context.PurchaseOrders
                .Include(o => o.Supplier)
                .Include(o => o.Employee)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var productSupplierOptions = _context.ProductSuppliers
                .Include(ps => ps.Product)
                .Include(ps => ps.Supplier)
                .Where(ps => ps.Product != null && ps.Supplier != null)
                .OrderBy(ps => ps.Product.Name)
                .ThenByDescending(ps => ps.IsDefault)
                .ThenBy(ps => ps.Supplier.CompanyName)
                .Select(ps => new
                {
                    productId = ps.ProductId,
                    productName = ps.Product.Name,
                    barcode = ps.Product.Barcode,
                    stock = ps.Product.StockQuantity,
                    supplierId = ps.SupplierId,
                    supplierName = ps.Supplier.CompanyName,
                    listPrice = ps.PurchasePrice,
                    discountRate = ps.Supplier.DiscountRate,
                    netPrice = Math.Round(ps.PurchasePrice - (ps.PurchasePrice * ps.Supplier.DiscountRate / 100), 2),
                    isDefault = ps.IsDefault
                })
                .ToList();

            ViewBag.ProductSupplierOptionsJson =
                System.Text.Json.JsonSerializer.Serialize(productSupplierOptions);

            return View();
        }

        [HttpPost]
        public IActionResult CreateManual(ManualPurchaseOrderViewModel model)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (model.Items == null || !model.Items.Any())
            {
                TempData["Error"] = "Sipariş oluşturmak için en az bir ürün eklemelisiniz.";
                return RedirectToAction("Create");
            }

            var selectedItems = model.Items
                .Where(x => x.ProductId > 0 && x.SupplierId > 0 && x.Quantity > 0)
                .ToList();

            if (!selectedItems.Any())
            {
                TempData["Error"] = "Sipariş oluşturmak için ürün, tedarikçi ve miktar bilgisi girmelisiniz.";
                return RedirectToAction("Create");
            }

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            var groupedBySupplier = selectedItems
                .GroupBy(x => x.SupplierId)
                .ToList();

            foreach (var supplierGroup in groupedBySupplier)
            {
                var orderItems = new List<PurchaseOrderItem>();

                foreach (var item in supplierGroup)
                {
                    var productSupplier = _context.ProductSuppliers
                        .Include(ps => ps.Supplier)
                        .FirstOrDefault(ps =>
                            ps.ProductId == item.ProductId &&
                            ps.SupplierId == item.SupplierId);

                    if (productSupplier == null)
                    {
                        TempData["Error"] = "Seçilen ürünlerden biri seçilen tedarikçiden alınamıyor.";
                        return RedirectToAction("Create");
                    }

                    decimal listUnitPrice = ParseDecimalValue(item.ListUnitPriceText);

                    if (listUnitPrice <= 0)
                    {
                        listUnitPrice = productSupplier.PurchasePrice;
                    }

                    decimal discountRate = productSupplier.Supplier?.DiscountRate ?? 0;

                    decimal netUnitPrice = listUnitPrice - (listUnitPrice * discountRate / 100);
                    netUnitPrice = Math.Round(netUnitPrice, 2);

                    orderItems.Add(new PurchaseOrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = netUnitPrice,
                        Subtotal = item.Quantity * netUnitPrice,
                        ReceivedQuantity = 0
                    });
                }

                var order = new PurchaseOrder
                {
                    SupplierId = supplierGroup.Key,
                    EmployeeId = employeeId,
                    OrderDate = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    Status = "Onay Bekliyor",
                    Note = model.Note,
                    TotalAmount = orderItems.Sum(x => x.Subtotal)
                };

                _context.PurchaseOrders.Add(order);
                _context.SaveChanges();

                foreach (var orderItem in orderItems)
                {
                    orderItem.PurchaseOrderId = order.Id;
                    _context.PurchaseOrderItems.Add(orderItem);
                }

                _context.SaveChanges();
            }

            TempData["Success"] = "Manuel satın alma siparişi oluşturuldu. Tedarikçi iskontoları uygulandı ve ürünler tedarikçiye göre ayrı siparişlere ayrıldı.";

            return RedirectToAction("Index");
        }

        private decimal ParseDecimalValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            value = value.Trim();

            if (decimal.TryParse(
                value,
                System.Globalization.NumberStyles.Any,
                new System.Globalization.CultureInfo("tr-TR"),
                out decimal trResult))
            {
                return trResult;
            }

            if (decimal.TryParse(
                value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal invariantResult))
            {
                return invariantResult;
            }

            return 0;
        }

        public IActionResult Suggestions()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var suppliers = _context.Suppliers
                .OrderBy(s => s.CompanyName)
                .ToList();

            var criticalProducts = _context.Products
                .Include(p => p.Category)
                    .ThenInclude(c => c.ParentCategory)
                .Include(p => p.Supplier)
                .Where(p => p.StockQuantity <= p.CriticalStock)
                .OrderBy(p => p.Supplier != null ? p.Supplier.CompanyName : "")
                .ThenBy(p => p.Name)
                .ToList();

            var model = new PurchaseOrderSuggestionsViewModel
            {
                Suppliers = suppliers,
                Items = criticalProducts.Select(p =>
                {
                    int targetStock = Math.Max(p.CriticalStock * 3, p.CriticalStock + 10);
                    int suggestedQuantity = targetStock - p.StockQuantity;

                    if (suggestedQuantity < 1)
                    {
                        suggestedQuantity = 1;
                    }

                    return new PurchaseOrderSuggestionItemViewModel
                    {
                        Selected = true,
                        ProductId = p.Id,
                        ProductName = p.Name,
                        Barcode = p.Barcode,
                        CategoryName = p.Category?.ParentCategory != null
                            ? p.Category.ParentCategory.Name + " > " + p.Category.Name
                            : p.Category?.Name ?? "-",
                        CurrentStock = p.StockQuantity,
                        CriticalStock = p.CriticalStock,
                        TargetStock = targetStock,
                        SuggestedQuantity = suggestedQuantity,
                        SupplierId = p.SupplierId,
                        SupplierName = p.Supplier?.CompanyName ?? "-",
                        UnitPrice = p.PurchasePrice
                    };
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult CreateFromSuggestions(PurchaseOrderSuggestionsViewModel model)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (model.Items == null)
            {
                TempData["Error"] = "Sipariş oluşturmak için ürün bulunamadı.";
                return RedirectToAction("Suggestions");
            }

            var selectedItems = model.Items
                .Where(x =>
                    x.Selected &&
                    x.ProductId > 0 &&
                    x.SupplierId != null &&
                    x.SuggestedQuantity > 0 &&
                    x.UnitPrice >= 0)
                .ToList();

            if (!selectedItems.Any())
            {
                TempData["Error"] = "Sipariş oluşturmak için en az bir ürün seçmelisiniz.";
                return RedirectToAction("Suggestions");
            }

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            var groupedBySupplier = selectedItems
                .GroupBy(x => x.SupplierId!.Value)
                .ToList();

            foreach (var group in groupedBySupplier)
            {
                var order = new PurchaseOrder
                {
                    SupplierId = group.Key,
                    EmployeeId = employeeId,
                    OrderDate = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    Status = "Onay Bekliyor",
                    Note = "Kritik stok ürünlerinden otomatik sipariş önerisiyle oluşturuldu.",
                    TotalAmount = group.Sum(x => x.SuggestedQuantity * x.UnitPrice)
                };

                _context.PurchaseOrders.Add(order);
                _context.SaveChanges();

                foreach (var item in group)
                {
                    var orderItem = new PurchaseOrderItem
                    {
                        PurchaseOrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.SuggestedQuantity,
                        UnitPrice = item.UnitPrice,
                        Subtotal = item.SuggestedQuantity * item.UnitPrice,
                        ReceivedQuantity = 0
                    };

                    _context.PurchaseOrderItems.Add(orderItem);
                }

                _context.SaveChanges();
            }

            TempData["Success"] = "Kritik stok ürünlerinden satın alma siparişi oluşturuldu. Siparişler onay bekliyor.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Approve(int id)
        {
            var order = _context.PurchaseOrders.Find(id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != "Onay Bekliyor")
            {
                TempData["Error"] = "Sadece onay bekleyen siparişler onaylanabilir.";
                return RedirectToAction("Index");
            }

            order.Status = "Onaylandı";
            order.ApprovedAt = DateTime.Now;

            _context.SaveChanges();

            TempData["Success"] = "Sipariş onaylandı.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ApproveSelected(List<int> selectedOrderIds)
        {
            if (selectedOrderIds == null || selectedOrderIds.Count == 0)
            {
                TempData["Error"] = "Onaylamak için en az bir sipariş seçmelisiniz.";
                return RedirectToAction("Index");
            }

            var orders = _context.PurchaseOrders
                .Where(o => selectedOrderIds.Contains(o.Id) && o.Status == "Onay Bekliyor")
                .ToList();

            if (!orders.Any())
            {
                TempData["Error"] = "Seçilen siparişler arasında onay bekleyen sipariş bulunamadı.";
                return RedirectToAction("Index");
            }

            foreach (var order in orders)
            {
                order.Status = "Onaylandı";
                order.ApprovedAt = DateTime.Now;
            }

            _context.SaveChanges();

            TempData["Success"] = $"{orders.Count} sipariş toplu olarak onaylandı.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ApproveAllPending()
        {
            var orders = _context.PurchaseOrders
                .Where(o => o.Status == "Onay Bekliyor")
                .ToList();

            if (!orders.Any())
            {
                TempData["Error"] = "Onay bekleyen sipariş bulunamadı.";
                return RedirectToAction("Index");
            }

            foreach (var order in orders)
            {
                order.Status = "Onaylandı";
                order.ApprovedAt = DateTime.Now;
            }

            _context.SaveChanges();

            TempData["Success"] = $"{orders.Count} onay bekleyen sipariş toplu olarak onaylandı.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Cancel(int id)
        {
            var order = _context.PurchaseOrders.Find(id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == "Teslim Alındı")
            {
                TempData["Error"] = "Teslim alınmış sipariş iptal edilemez.";
                return RedirectToAction("Details", new { id });
            }

            order.Status = "İptal Edildi";
            _context.SaveChanges();

            TempData["Success"] = "Sipariş iptal edildi.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Receive(int id, Dictionary<int, int> receivedQuantities)
        {
            var order = _context.PurchaseOrders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != "Onaylandı" && order.Status != "Kısmi Teslim Alındı")
            {
                TempData["Error"] = "Ürün teslim almak için sipariş önce onaylanmalıdır.";
                return RedirectToAction("Details", new { id });
            }

            foreach (var item in order.Items)
            {
                int newReceivedQuantity = 0;

                if (receivedQuantities != null && receivedQuantities.ContainsKey(item.Id))
                {
                    newReceivedQuantity = receivedQuantities[item.Id];
                }

                if (newReceivedQuantity < 0)
                {
                    newReceivedQuantity = 0;
                }

                if (newReceivedQuantity > item.Quantity)
                {
                    newReceivedQuantity = item.Quantity;
                }

                int stockIncrease = newReceivedQuantity - item.ReceivedQuantity;

                if (stockIncrease > 0 && item.Product != null)
                {
                    item.Product.StockQuantity += stockIncrease;
                    item.ReceivedQuantity = newReceivedQuantity;
                }
            }

            bool allReceived = order.Items.All(i => i.ReceivedQuantity >= i.Quantity);
            bool anyReceived = order.Items.Any(i => i.ReceivedQuantity > 0);

            if (allReceived)
            {
                order.Status = "Teslim Alındı";
                order.CheckedAt = DateTime.Now;
                order.ReceivedAt = DateTime.Now;
            }
            else if (anyReceived)
            {
                order.Status = "Kısmi Teslim Alındı";
                order.CheckedAt = DateTime.Now;
            }

            _context.SaveChanges();

            TempData["Success"] = "Teslim alma işlemi kaydedildi. Gelen miktarlar stoğa eklendi.";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public IActionResult ReceiveAll(int id)
        {
            var order = _context.PurchaseOrders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != "Onaylandı" && order.Status != "Kısmi Teslim Alındı")
            {
                TempData["Error"] = "Sipariş geldi onayı verebilmek için sipariş önce onaylanmış olmalıdır.";
                return RedirectToAction("Index");
            }

            foreach (var item in order.Items)
            {
                int remainingQuantity = item.Quantity - item.ReceivedQuantity;

                if (remainingQuantity > 0 && item.Product != null)
                {
                    item.Product.StockQuantity += remainingQuantity;
                    item.ReceivedQuantity = item.Quantity;
                }
            }

            order.Status = "Teslim Alındı";
            order.CheckedAt = DateTime.Now;
            order.ReceivedAt = DateTime.Now;

            _context.SaveChanges();

            TempData["Success"] = "Sipariş geldi olarak onaylandı. Ürünler kontrol edildi ve stoklara işlendi.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ReceiveSelected(List<int> selectedReceiveOrderIds)
        {
            if (selectedReceiveOrderIds == null || selectedReceiveOrderIds.Count == 0)
            {
                TempData["Error"] = "Geldi olarak işaretlemek için en az bir sipariş seçmelisiniz.";
                return RedirectToAction("Index");
            }

            var orders = _context.PurchaseOrders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o =>
                    selectedReceiveOrderIds.Contains(o.Id) &&
                    (o.Status == "Onaylandı" || o.Status == "Kısmi Teslim Alındı"))
                .ToList();

            if (!orders.Any())
            {
                TempData["Error"] = "Seçilen siparişler arasında geldi olarak işlenebilecek sipariş bulunamadı.";
                return RedirectToAction("Index");
            }

            foreach (var order in orders)
            {
                foreach (var item in order.Items)
                {
                    int remainingQuantity = item.Quantity - item.ReceivedQuantity;

                    if (remainingQuantity > 0 && item.Product != null)
                    {
                        item.Product.StockQuantity += remainingQuantity;
                        item.ReceivedQuantity = item.Quantity;
                    }
                }

                order.Status = "Teslim Alındı";
                order.CheckedAt = DateTime.Now;
                order.ReceivedAt = DateTime.Now;
            }

            _context.SaveChanges();

            TempData["Success"] = $"{orders.Count} sipariş geldi/kontrol edildi olarak işaretlendi ve stoklara işlendi.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ReceiveAllApproved()
        {
            var orders = _context.PurchaseOrders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.Status == "Onaylandı" || o.Status == "Kısmi Teslim Alındı")
                .ToList();

            if (!orders.Any())
            {
                TempData["Error"] = "Geldi olarak işlenebilecek onaylı sipariş bulunamadı.";
                return RedirectToAction("Index");
            }

            foreach (var order in orders)
            {
                foreach (var item in order.Items)
                {
                    int remainingQuantity = item.Quantity - item.ReceivedQuantity;

                    if (remainingQuantity > 0 && item.Product != null)
                    {
                        item.Product.StockQuantity += remainingQuantity;
                        item.ReceivedQuantity = item.Quantity;
                    }
                }

                order.Status = "Teslim Alındı";
                order.CheckedAt = DateTime.Now;
                order.ReceivedAt = DateTime.Now;
            }

            _context.SaveChanges();

            TempData["Success"] = $"{orders.Count} onaylı sipariş geldi/kontrol edildi olarak işaretlendi ve stoklara işlendi.";
            return RedirectToAction("Index");
        }
    }

    public class PurchaseOrderSuggestionsViewModel
    {
        public List<Supplier> Suppliers { get; set; } = new List<Supplier>();

        public List<PurchaseOrderSuggestionItemViewModel> Items { get; set; } = new List<PurchaseOrderSuggestionItemViewModel>();
    }

    public class PurchaseOrderSuggestionItemViewModel
    {
        public bool Selected { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = "";

        public string Barcode { get; set; } = "";

        public string CategoryName { get; set; } = "";

        public int CurrentStock { get; set; }

        public int CriticalStock { get; set; }

        public int TargetStock { get; set; }

        public int SuggestedQuantity { get; set; }

        public int? SupplierId { get; set; }

        public string SupplierName { get; set; } = "";

        public decimal UnitPrice { get; set; }
    }

    public class ManualPurchaseOrderViewModel
    {
        public string? Note { get; set; }

        public List<ManualPurchaseOrderItemViewModel> Items { get; set; } = new List<ManualPurchaseOrderItemViewModel>();
    }

    public class ManualPurchaseOrderItemViewModel
    {
        public int ProductId { get; set; }

        public int SupplierId { get; set; }

        public int Quantity { get; set; }

        public string? ListUnitPriceText { get; set; }
    }
}