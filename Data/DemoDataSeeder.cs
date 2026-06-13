using MarketERP.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MarketERP.Data;

public static class DemoDataSeeder
{
    public static async Task SeedDemoDataAsync(AppDbContext context)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        await ClearDemoDataAsync(context);

        var employees = await SeedEmployeesAsync(context);
        var categories = await SeedCategoriesAsync(context);
        var suppliers = await SeedSuppliersAsync(context);
        var products = await SeedProductsAsync(context, categories, suppliers);
        var customers = await SeedCustomersAsync(context);

        ValidateBaseDemoData(employees, products, customers, suppliers);

        await SeedSalesAsync(context, employees, products, customers);
        await SeedWholesaleSalesAsync(context, employees, products, customers);
        await SeedReturnRequestsAsync(context, employees);
        await SeedPurchaseOrdersAsync(context, employees, products, suppliers);
        NormalizeDemoStockLevels(products);
        await SeedOperationalDataAsync(context, employees);
        await DbSeeder.EnsureSavedQueriesAsync(context);
        await ValidateSeedCountsAsync(context);

        await transaction.CommitAsync();
    }

    private static async Task ClearDemoDataAsync(AppDbContext context)
    {
        await context.ReturnRequests.ExecuteDeleteAsync();
        await context.WholesaleSaleRequestItems.ExecuteDeleteAsync();
        await context.WholesaleSaleRequests.ExecuteDeleteAsync();
        await context.SaleDetails.ExecuteDeleteAsync();
        await context.Sales.ExecuteDeleteAsync();
        await context.CashRegisterClosings.ExecuteDeleteAsync();
        await context.PurchaseOrderItems.ExecuteDeleteAsync();
        await context.PurchaseOrders.ExecuteDeleteAsync();
        await context.ProductSuppliers.ExecuteDeleteAsync();
        await ClearLegacyStockMovementsAsync(context);
        await context.EmployeeBonuses.ExecuteDeleteAsync();
        await context.EmployeeLeaves.ExecuteDeleteAsync();
        await context.EmployeeShifts.ExecuteDeleteAsync();
        await context.SupportTickets.ExecuteDeleteAsync();
        await context.Expenses.ExecuteDeleteAsync();
        await context.SavedQueries.ExecuteDeleteAsync();
        await context.UserRoles.ExecuteDeleteAsync();
        await context.Products.ExecuteDeleteAsync();
        await context.Categories
            .Where(c => c.ParentCategoryId != null)
            .ExecuteDeleteAsync();
        await context.Categories.ExecuteDeleteAsync();
        await context.Customers.ExecuteDeleteAsync();
        await context.Suppliers.ExecuteDeleteAsync();
        await context.Employees.ExecuteDeleteAsync();

        // ExecuteDelete bypasses EF's tracker; detach entities loaded by DbSeeder.
        context.ChangeTracker.Clear();
    }

    private static async Task ClearLegacyStockMovementsAsync(AppDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();
        command.CommandText = """
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'stock_movements';
            """;

        var tableExists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
        if (!tableExists)
        {
            return;
        }

        command.CommandText = "DELETE FROM `stock_movements`;";
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<Dictionary<string, Employee>> SeedEmployeesAsync(
        AppDbContext context)
    {
        var definitions = new[]
        {
            new EmployeeSeed("Sistem Yöneticisi", "admin", "Admin", 65000m, -8),
            new EmployeeSeed("Hakan Bozkurt", "mudur", "Mağaza Müdürü", 52000m, -10),
            new EmployeeSeed("Ayşe Yılmaz", "kasiyer", "Kasiyer", 30000m, -3),
            new EmployeeSeed("Mert Kaya", "toptanci", "Toptan Satış Sorumlusu", 36000m, -5),
            new EmployeeSeed("Selin Acar", "muhasebe", "Muhasebe", 42000m, -7),
            new EmployeeSeed("Emre Demir", "depocu", "Depo Sorumlusu", 34000m, -4),
            new EmployeeSeed("Deniz Aksoy", "kasiyer2", "Kasiyer", 30000m, -2),
            new EmployeeSeed("Ceren Yıldız", "kasiyer3", "Kasiyer", 30500m, -4),
            new EmployeeSeed("Onur Şen", "toptanci2", "Toptan Satış Sorumlusu", 35500m, -3),
            new EmployeeSeed("Burcu Koç", "depocu2", "Depo Sorumlusu", 33500m, -2)
        };

        var roles = await context.Roles
            .Where(r => definitions.Select(d => d.RoleName).Contains(r.Name))
            .ToDictionaryAsync(r => r.Name);

        var missingRoles = definitions
            .Select(d => d.RoleName)
            .Where(roleName => !roles.ContainsKey(roleName))
            .ToList();
        if (missingRoles.Count > 0)
        {
            throw new InvalidOperationException(
                $"Demo kullanıcı rolleri bulunamadı: {string.Join(", ", missingRoles)}");
        }

        var passwordHasher = new PasswordHasher<Employee>();
        var employees = new Dictionary<string, Employee>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in definitions)
        {
            var employee = new Employee
            {
                FullName = definition.FullName,
                Phone = $"0532 555 {1000 + employees.Count:0000}",
                Position = definition.RoleName,
                Salary = definition.Salary,
                HireDate = DateTime.Today.AddYears(definition.HireYearOffset),
                Email = $"{definition.Username}@marketerp.demo",
                Username = definition.Username,
                IsActive = true
            };
            employee.Password = passwordHasher.HashPassword(employee, "1234");

            context.Employees.Add(employee);
            employees[definition.Username] = employee;
        }

        await context.SaveChangesAsync();

        foreach (var definition in definitions)
        {
            context.UserRoles.Add(new UserRole
            {
                EmployeeId = employees[definition.Username].Id,
                RoleId = roles[definition.RoleName].Id
            });
        }

        await context.SaveChangesAsync();
        return employees;
    }

    private static async Task<Dictionary<string, Category>> SeedCategoriesAsync(
        AppDbContext context)
    {
        var definitions = new[]
        {
            new CategorySeed("Süt & Kahvaltılık", 1m),
            new CategorySeed("Ekmek & Unlu Mamuller", 1m),
            new CategorySeed("Temel Gıda", 1m),
            new CategorySeed("Bakliyat & Konserve", 1m),
            new CategorySeed("Yağ & Sos", 1m),
            new CategorySeed("Çay & Kahve", 10m),
            new CategorySeed("Su & İçecek", 10m),
            new CategorySeed("Atıştırmalık", 10m),
            new CategorySeed("Çikolata & Şekerleme", 10m),
            new CategorySeed("Meyve & Sebze", 1m),
            new CategorySeed("Temizlik", 20m),
            new CategorySeed("Kişisel Bakım", 20m),
            new CategorySeed("Kağıt Ürünleri", 20m),
            new CategorySeed("Bebek Ürünleri", 10m),
            new CategorySeed("Ev & Mutfak", 20m)
        };

        var categories = definitions
            .Select(d => new Category
            {
                Name = d.Name,
                DefaultVatRate = d.VatRate
            })
            .ToList();

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
        return categories.ToDictionary(c => c.Name);
    }

    private static async Task<List<Supplier>> SeedSuppliersAsync(AppDbContext context)
    {
        var names = new[]
        {
            "Marmara Süt Ürünleri A.Ş.",
            "Bereket Unlu Mamuller Dağıtım",
            "Anadolu Kuru Gıda",
            "Hasat Bakliyat ve Konserve",
            "Ege Yağ ve Zeytin",
            "Karadeniz Çay Kahve",
            "Pınarbaşı İçecek Dağıtım",
            "Lezzet Atıştırmalık",
            "Tatlı Dünya Gıda",
            "Taze Tarım Sebze Meyve",
            "Hijyen Temizlik Sistemleri",
            "Bakım Kişisel Ürünler",
            "Pak Kağıt Tedarik",
            "Minik Dünya Bebek Ürünleri",
            "Pratik Ev Gereçleri",
            "Trakya Et ve Tavuk Dağıtım",
            "Akdeniz Meyve Sebze Lojistik",
            "Serin Zincir İçecek",
            "Doğa Organik Gıda",
            "Güven Ambalaj ve Kargo",
            "Profesyonel Market Temizlik",
            "Marmara Dondurulmuş Gıda"
        };

        var suppliers = names.Select((name, index) => new Supplier
        {
            CompanyName = name,
            Phone = $"0212 444 {20 + index:00} {30 + index:00}",
            Email = $"tedarik{index + 1}@marketerp.demo",
            Address = $"İstanbul Demo Dağıtım Bölgesi No: {index + 1}",
            DiscountRate = index % 5 == 0 ? 7.5m : index % 3 == 0 ? 5m : 3m
        }).ToList();

        context.Suppliers.AddRange(suppliers);
        await context.SaveChangesAsync();
        return suppliers;
    }

    private static async Task<List<Product>> SeedProductsAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, Category> categories,
        IReadOnlyList<Supplier> suppliers)
    {
        var catalog = GetProductCatalog();
        var categoryOrder = categories.Keys.ToList();
        var categoryCounts = new Dictionary<string, int>();
        var products = new List<Product>(catalog.Count);

        for (var index = 0; index < catalog.Count; index++)
        {
            var definition = catalog[index];
            var categoryIndex = categoryOrder.IndexOf(definition.Category);
            var sequence = categoryCounts.GetValueOrDefault(definition.Category);
            categoryCounts[definition.Category] = sequence + 1;

            var basePrice = GetCategoryBasePrice(definition.Category);
            var purchasePrice = Math.Round(basePrice + sequence * 8.50m, 2);
            var profitMargin = 1.18m + index % 5 * 0.06m;
            var salePrice = Math.Round(purchasePrice * profitMargin, 2);
            var criticalStock = 8 + index % 8;
            var stockQuantity = index % 23 == 0
                ? 0
                : index % 11 == 0
                    ? Math.Max(1, criticalStock - 3)
                    : 120 + index * 11 % 140;
            var supplier = suppliers[categoryIndex % suppliers.Count];

            products.Add(new Product
            {
                Category = categories[definition.Category],
                CategoryId = categories[definition.Category].Id,
                SupplierId = supplier.Id,
                Barcode = (8690000000000L + index + 1).ToString(),
                Name = definition.Name,
                PurchasePrice = purchasePrice,
                SalePrice = salePrice,
                StockQuantity = stockQuantity,
                CriticalStock = criticalStock,
                VatRateOverride = null,
                CreatedAt = DateTime.Today.AddDays(-(index % 120))
            });
        }

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        for (var index = 0; index < products.Count; index++)
        {
            var product = products[index];
            var defaultSupplierId = product.SupplierId!.Value;
            context.ProductSuppliers.Add(new ProductSupplier
            {
                ProductId = product.Id,
                SupplierId = defaultSupplierId,
                PurchasePrice = product.PurchasePrice,
                IsDefault = true,
                MinOrderQuantity = 6 + index % 10,
                LeadTimeDays = 1 + index % 7
            });

            if (index % 5 == 0)
            {
                var alternateSupplier = suppliers[(index + 3) % suppliers.Count];
                if (alternateSupplier.Id != defaultSupplierId)
                {
                    context.ProductSuppliers.Add(new ProductSupplier
                    {
                        ProductId = product.Id,
                        SupplierId = alternateSupplier.Id,
                        PurchasePrice = Math.Round(product.PurchasePrice * 1.03m, 2),
                        IsDefault = false,
                        MinOrderQuantity = 12,
                        LeadTimeDays = 3 + index % 5
                    });
                }
            }
        }

        await context.SaveChangesAsync();
        return products;
    }

    private static async Task<List<Customer>> SeedCustomersAsync(AppDbContext context)
    {
        var customers = new List<Customer>
        {
            new()
            {
                FullName = "Nihai Tüketici",
                Phone = "-",
                Email = null,
                Address = "Mağaza Satış Müşterisi",
                DiscountRate = 0
            }
        };

        var firstNames = new[]
        {
            "Ahmet", "Ayşe", "Mehmet", "Fatma", "Mustafa",
            "Zeynep", "Emre", "Elif", "Burak", "Selin",
            "Can", "Derya", "Kerem", "Melis", "Okan"
        };
        var lastNames = new[]
        {
            "Yılmaz", "Kaya", "Demir", "Şahin", "Çelik", "Aydın", "Arslan"
        };
        var individualDiscounts = new[] { 0m, 0m, 0m, 3m, 5m };

        foreach (var firstName in firstNames)
        {
            foreach (var lastName in lastNames)
            {
                var index = customers.Count;
                customers.Add(new Customer
                {
                    FullName = $"{firstName} {lastName}",
                    Phone = $"05{30 + index % 20} 555 {1000 + index:0000}",
                    Email = $"musteri{index}@example.demo",
                    Address = $"İstanbul Demo Mahallesi No: {index}",
                    DiscountRate = individualDiscounts[index % individualDiscounts.Length]
                });
            }
        }

        var corporateNames = new[]
        {
            "Örnek Büfe", "Merkez Kafe", "Güneş Lokantası", "Bereket Market",
            "Dostlar Bakkaliyesi", "Şehir Otel", "Mavi Restoran", "Lezzet Catering",
            "Umut Kantin", "Çınar Pastanesi", "Yıldız Kuruyemiş", "Vadi Cafe",
            "Park Büfe", "Sahil Restoran", "Anadolu Yemek", "Pera Otel",
            "Köşe Market", "Ada Kafe", "Nehir Catering", "Zirve Kantin",
            "Bahar Pastanesi", "Kent Büfe", "İstanbul Gıda", "Marmara Lokantası",
            "Ekin Market", "Lale Cafe", "Başak Toptan", "Rota Otel", "Göl Restoran"
        };
        var corporateDiscounts = new[] { 7.5m, 10m, 12.5m, 15m, 5m };

        foreach (var companyName in corporateNames)
        {
            var index = customers.Count;
            customers.Add(new Customer
            {
                FullName = companyName,
                Phone = $"0216 555 {index:0000}",
                Email = $"kurumsal{index}@example.demo",
                Address = $"İstanbul Ticaret Bölgesi No: {index}",
                DiscountRate = corporateDiscounts[index % corporateDiscounts.Length]
            });
        }

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();
        return customers;
    }

    private static async Task SeedSalesAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, Employee> employees,
        IReadOnlyList<Product> products,
        IReadOnlyList<Customer> customers)
    {
        var cashiers = employees.Values
            .Where(e => e.Position == "Kasiyer")
            .OrderBy(e => e.Id)
            .ToList();
        var paymentTypes = new[] { "Nakit", "Kart", "Havale", "Yemek Kartı" };
        var detailPlans = new List<(Sale Sale, Product Product, int Quantity)>();

        for (var saleIndex = 0; saleIndex < 400; saleIndex++)
        {
            var itemCount = 3 + saleIndex % 6;
            var saleItems = new List<(Product Product, int Quantity)>();

            for (var itemIndex = 0; itemIndex < itemCount; itemIndex++)
            {
                var product = products[(saleIndex * 7 + itemIndex * 19) % products.Count];
                var requestedQuantity = 2 + (saleIndex + itemIndex) % 6;
                var quantity = Math.Min(requestedQuantity, Math.Max(product.StockQuantity, 0));
                if (quantity == 0)
                {
                    continue;
                }

                product.StockQuantity -= quantity;
                saleItems.Add((product, quantity));
            }

            if (saleItems.Count == 0)
            {
                var fallback = products.First(p => p.StockQuantity > 0);
                fallback.StockQuantity--;
                saleItems.Add((fallback, 1));
            }

            var sale = new Sale
            {
                CustomerId = saleIndex % 4 == 0
                    ? customers[0].Id
                    : customers[1 + saleIndex % (customers.Count - 1)].Id,
                EmployeeId = cashiers[saleIndex % cashiers.Count].Id,
                SaleDate = GetRetailSaleDate(saleIndex),
                TotalAmount = saleItems.Sum(i => i.Product.SalePrice * i.Quantity),
                PaymentType = paymentTypes[(saleIndex * 5) % paymentTypes.Length]
            };
            context.Sales.Add(sale);
            detailPlans.AddRange(saleItems.Select(i => (sale, i.Product, i.Quantity)));
        }

        await context.SaveChangesAsync();
        context.SaleDetails.AddRange(detailPlans.Select(plan =>
            CreateSaleDetail(plan.Sale.Id, plan.Product, plan.Quantity)));
        await context.SaveChangesAsync();
    }

    private static SaleDetail CreateSaleDetail(int saleId, Product product, int quantity)
    {
        return CreateSaleDetail(saleId, product, quantity, product.SalePrice);
    }

    private static SaleDetail CreateSaleDetail(
        int saleId,
        Product product,
        int quantity,
        decimal unitPrice)
    {
        var subtotal = unitPrice * quantity;
        var vatRate = product.VatRateOverride ?? product.Category?.DefaultVatRate ?? 1m;
        var vatBase = Math.Round(subtotal / (1 + vatRate / 100), 2);

        return new SaleDetail
        {
            SaleId = saleId,
            ProductId = product.Id,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Subtotal = subtotal,
            AppliedVatRate = vatRate,
            VatBase = vatBase,
            VatAmount = subtotal - vatBase
        };
    }

    private static async Task SeedWholesaleSalesAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, Employee> employees,
        IReadOnlyList<Product> products,
        IReadOnlyList<Customer> customers)
    {
        var statuses = new[]
        {
            "Beklemede", "Onaylandı", "Reddedildi", "Hazırlanıyor", "Teslim Edildi"
        };
        var paymentTypes = new[] { "Havale", "Kart", "Nakit" };
        var wholesaleCustomers = customers.Where(c => c.DiscountRate >= 5m).ToList();
        var wholesaleEmployees = employees.Values
            .Where(e => e.Position == "Toptan Satış Sorumlusu")
            .OrderBy(e => e.Id)
            .ToList();

        for (var requestIndex = 0; requestIndex < 60; requestIndex++)
        {
            var customer = wholesaleCustomers[requestIndex % wholesaleCustomers.Count];
            var requestDate = DateTime.Now.AddDays(-(requestIndex * 4 % 175))
                .AddHours(-(requestIndex % 8));
            var status = statuses[requestIndex % statuses.Length];
            var discountRate = customer.DiscountRate;
            var originalSubtotal = 0m;
            var requestItems = new List<WholesaleSaleRequestItem>();

            var itemCount = 4 + requestIndex % 3;
            for (var itemIndex = 0; itemIndex < itemCount; itemIndex++)
            {
                var product = products[(requestIndex * 11 + itemIndex * 23) % products.Count];
                var quantity = 20 + (requestIndex + itemIndex * 5) % 26;
                var discountedUnitPrice = Math.Round(
                    product.SalePrice * (1 - discountRate / 100),
                    2);
                originalSubtotal += product.SalePrice * quantity;
                requestItems.Add(new WholesaleSaleRequestItem
                {
                    ProductId = product.Id,
                    Quantity = quantity,
                    UnitPrice = discountedUnitPrice,
                    Subtotal = discountedUnitPrice * quantity
                });
            }

            var total = requestItems.Sum(i => i.Subtotal);
            var request = new WholesaleSaleRequest
            {
                CustomerId = customer.Id,
                EmployeeId = wholesaleEmployees[requestIndex % wholesaleEmployees.Count].Id,
                RequestDate = requestDate,
                DueDate = requestDate.AddDays(14),
                DeliveryDate = status == "Teslim Edildi" ? requestDate.AddDays(4) : null,
                OfferValidUntil = requestDate.AddDays(7),
                DeliveryAddress = customer.Address,
                PaymentType = paymentTypes[requestIndex % paymentTypes.Length],
                DiscountRate = discountRate,
                SubtotalAmount = originalSubtotal,
                DiscountAmount = originalSubtotal - total,
                TotalAmount = total,
                Status = status,
                Note = "Demo toptan satış talebi",
                ApprovedAt = status is "Onaylandı" or "Hazırlanıyor" or "Teslim Edildi"
                    ? requestDate.AddDays(1)
                    : null,
                RejectedAt = status == "Reddedildi" ? requestDate.AddDays(1) : null,
                ReviewNote = status == "Reddedildi"
                    ? "Stok ve teslim tarihi koşulları uygun bulunmadı."
                    : "Demo yönetici değerlendirmesi",
                Items = requestItems
            };
            context.WholesaleSaleRequests.Add(request);
            await context.SaveChangesAsync();

            if (status is not ("Onaylandı" or "Hazırlanıyor" or "Teslim Edildi"))
            {
                continue;
            }

            var sale = new Sale
            {
                CustomerId = customer.Id,
                EmployeeId = request.EmployeeId,
                SaleDate = requestDate.AddDays(status == "Teslim Edildi" ? 4 : 2),
                TotalAmount = total,
                PaymentType = request.PaymentType ?? "Havale"
            };
            context.Sales.Add(sale);
            await context.SaveChangesAsync();

            foreach (var item in requestItems)
            {
                var product = products.First(p => p.Id == item.ProductId);
                context.SaleDetails.Add(
                    CreateSaleDetail(sale.Id, product, item.Quantity, item.UnitPrice));

                if (status == "Teslim Edildi")
                {
                    product.StockQuantity = Math.Max(0, product.StockQuantity - item.Quantity);
                }
            }

            request.SaleId = sale.Id;
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedReturnRequestsAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, Employee> employees)
    {
        var details = await context.SaleDetails
            .Include(d => d.Sale)
            .OrderByDescending(d => d.Sale!.SaleDate)
            .Take(90)
            .ToListAsync();
        var statuses = new[] { "Beklemede", "Onaylandı", "Reddedildi" };
        var reasons = new[]
        {
            "Hasarlı ürün", "Yanlış ürün", "Son kullanma tarihi",
            "Müşteri vazgeçti", "Fiyat farkı"
        };
        var reviewEmployees = employees.Values
            .Where(e => e.Position == "Kasiyer")
            .OrderBy(e => e.Id)
            .ToList();

        for (var index = 0; index < 45; index++)
        {
            var detail = details[(index * 2) % details.Count];
            var status = statuses[index % statuses.Length];
            var requestedAt = detail.Sale!.SaleDate.AddDays(1 + index % 5);

            context.ReturnRequests.Add(new ReturnRequest
            {
                RequestNo = $"IADE-{DateTime.Today:yyyyMMdd}-{index + 1:000}",
                SaleId = detail.SaleId,
                SaleDetailId = detail.Id,
                ProductId = detail.ProductId,
                EmployeeId = reviewEmployees[index % reviewEmployees.Count].Id,
                Quantity = Math.Min(detail.Quantity, 1 + index % 2),
                ReasonType = reasons[index % reasons.Length],
                Reason = $"{reasons[index % reasons.Length]} nedeniyle müşteri iade talebi.",
                Status = status,
                RequestedAt = requestedAt,
                ReviewedAt = status == "Beklemede" ? null : requestedAt.AddHours(6),
                ReviewNote = status == "Onaylandı"
                    ? "Ürün ve satış kaydı kontrol edilerek onaylandı."
                    : status == "Reddedildi"
                        ? "İade koşulları karşılanmadığı için reddedildi."
                        : null
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedPurchaseOrdersAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, Employee> employees,
        IReadOnlyList<Product> products,
        IReadOnlyList<Supplier> suppliers)
    {
        var statuses = new[]
        {
            "Beklemede", "Onay Bekliyor", "Onaylandı", "Teslim Alındı", "İptal"
        };
        var warehouseEmployees = employees.Values
            .Where(e => e.Position == "Depo Sorumlusu")
            .OrderBy(e => e.Id)
            .ToList();

        for (var orderIndex = 0; orderIndex < 56; orderIndex++)
        {
            var status = statuses[orderIndex % statuses.Length];
            var items = Enumerable.Range(0, 4)
                .Select(itemIndex =>
                {
                    var product = products[(orderIndex * 7 + itemIndex * 11) % products.Count];
                    var quantity = 12 + (orderIndex + itemIndex) % 20;
                    var received = status == "Teslim Alındı" ? quantity : 0;

                    return new PurchaseOrderItem
                    {
                        ProductId = product.Id,
                        Quantity = quantity,
                        UnitPrice = product.PurchasePrice,
                        Subtotal = product.PurchasePrice * quantity,
                        ReceivedQuantity = received
                    };
                })
                .ToList();

            var orderDate = DateTime.Now.AddDays(-(orderIndex * 7 % 175));
            var order = new PurchaseOrder
            {
                SupplierId = suppliers[orderIndex % suppliers.Count].Id,
                EmployeeId = warehouseEmployees[orderIndex % warehouseEmployees.Count].Id,
                OrderDate = orderDate,
                ApprovedAt = status is "Onaylandı" or "Teslim Alındı"
                    ? orderDate.AddDays(1)
                    : null,
                CheckedAt = status == "Teslim Alındı" ? orderDate.AddDays(3) : null,
                ReceivedAt = status == "Teslim Alındı" ? orderDate.AddDays(3) : null,
                Status = status,
                Note = status == "İptal"
                    ? "Tedarik süresi uygun olmadığı için iptal edildi."
                    : "Demo satın alma ve mal kabul siparişi",
                TotalAmount = items.Sum(i => i.Subtotal),
                CreatedAt = orderDate,
                Items = items
            };
            context.PurchaseOrders.Add(order);

            if (status == "Teslim Alındı")
            {
                foreach (var item in items)
                {
                    products.First(p => p.Id == item.ProductId).StockQuantity += item.ReceivedQuantity;
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedOperationalDataAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, Employee> employees)
    {
        var expenseNames = new[]
        {
            "Kira", "Elektrik faturası", "Su faturası", "Doğalgaz faturası",
            "İnternet faturası", "Personel maaşı", "SGK / sigorta", "Vergi",
            "Tedarikçi ödemesi", "Bakım / onarım", "Temizlik gideri",
            "Kargo / lojistik", "Ofis / kırtasiye", "Diğer gider"
        };
        context.Expenses.AddRange(Enumerable.Range(0, 70).Select(index => new Expense
        {
            Title = expenseNames[index % expenseNames.Length],
            Amount = GetExpenseAmount(expenseNames[index % expenseNames.Length], index),
            ExpenseDate = DateTime.Today.AddDays(-(index * 4 % 180)),
            Description = "Demo ERP gider kaydı ve ödeme açıklaması"
        }));

        var monday = DateTime.Today.AddDays(
            -((7 + (DateTime.Today.DayOfWeek - DayOfWeek.Monday)) % 7));
        var shiftEmployees = employees.Values
            .Where(e => e.Position != "Admin")
            .OrderBy(e => e.Id)
            .ToList();
        var shiftTypes = new[]
        {
            ("Sabah", new TimeSpan(8, 0, 0), new TimeSpan(16, 0, 0)),
            ("Akşam", new TimeSpan(16, 0, 0), TimeSpan.Zero),
            ("Gece", TimeSpan.Zero, new TimeSpan(8, 0, 0)),
            ("Tam gün", new TimeSpan(9, 0, 0), new TimeSpan(18, 0, 0))
        };
        for (var weekOffset = -1; weekOffset <= 3; weekOffset++)
        {
            foreach (var employee in shiftEmployees)
            {
                for (var day = 0; day < 7; day++)
                {
                    var shiftDate = monday.AddDays(weekOffset * 7 + day);
                    if ((employee.Id + day + weekOffset) % 11 == 0)
                    {
                        context.EmployeeShifts.Add(new EmployeeShift
                        {
                            EmployeeId = employee.Id,
                            ShiftDate = shiftDate,
                            StartTime = TimeSpan.Zero,
                            EndTime = TimeSpan.Zero,
                            Description = "İzinli"
                        });
                        continue;
                    }

                    var shift = shiftTypes[
                        Math.Abs(employee.Id + day + weekOffset) % shiftTypes.Length];
                    context.EmployeeShifts.Add(new EmployeeShift
                    {
                        EmployeeId = employee.Id,
                        ShiftDate = shiftDate,
                        StartTime = shift.Item2,
                        EndTime = shift.Item3,
                        Description = shift.Item1
                    });
                }
            }
        }

        var leaveReasons = new[]
        {
            "Yıllık izin", "Hastalık izni", "Mazeret izni", "Ailevi nedenler"
        };
        var leaveStatuses = new[] { "Beklemede", "Onaylandı", "Reddedildi" };
        var priorityLeaveEmployees = new[]
        {
            employees["kasiyer"], employees["toptanci"], employees["depocu"]
        };
        for (var index = 0; index < 60; index++)
        {
            var employee = index < priorityLeaveEmployees.Length
                ? priorityLeaveEmployees[index]
                : shiftEmployees[index % shiftEmployees.Count];
            var startDate = DateTime.Today.AddDays(-150 + index * 3);
            context.EmployeeLeaves.Add(new EmployeeLeave
            {
                EmployeeId = employee.Id,
                StartDate = startDate,
                EndDate = startDate.AddDays(index % 4),
                LeaveReason = leaveReasons[index % leaveReasons.Length],
                Status = index < priorityLeaveEmployees.Length
                    ? "Beklemede"
                    : leaveStatuses[index % leaveStatuses.Length]
            });
        }

        var bonusEmployees = employees.Values
            .Where(e => e.Position != "Admin" && e.Position != "Mağaza Müdürü")
            .OrderBy(e => e.Id)
            .ToList();
        for (var index = 0; index < 36; index++)
        {
            context.EmployeeBonuses.Add(new EmployeeBonus
            {
                EmployeeId = bonusEmployees[index % bonusEmployees.Count].Id,
                BonusAmount = 750m + index % 6 * 250m,
                BonusDate = DateTime.Today.AddDays(-(index * 7 % 170)),
                Description = index % 3 == 0
                    ? "Aylık performans primi"
                    : index % 3 == 1
                        ? "Satış hedefi primi"
                        : "Operasyon başarı primi"
            });
        }

        var recentSales = await context.Sales
            .Where(s => s.SaleDate >= DateTime.Today.AddDays(-40))
            .ToListAsync();
        var cashierIds = employees.Values
            .Where(e => e.Position == "Kasiyer")
            .OrderBy(e => e.Id)
            .Select(e => e.Id)
            .ToList();
        for (var dayIndex = 0; dayIndex < 40; dayIndex++)
        {
            var closingDay = DateTime.Today.AddDays(-dayIndex);
            var daySales = recentSales
                .Where(s => s.SaleDate.Date == closingDay)
                .ToList();
            var cashTotal = daySales
                .Where(s => s.PaymentType == "Nakit")
                .Sum(s => s.TotalAmount);
            var nonCashTotal = daySales
                .Where(s => s.PaymentType != "Nakit")
                .Sum(s => s.TotalAmount);
            var difference = dayIndex % 7 == 0 ? -12.50m : dayIndex % 9 == 0 ? 8.75m : 0m;

            context.CashRegisterClosings.Add(new CashRegisterClosing
            {
                EmployeeId = cashierIds[dayIndex % cashierIds.Count],
                ClosingDate = closingDay.AddHours(21),
                CashSalesTotal = cashTotal,
                CardSalesTotal = nonCashTotal,
                TotalSalesAmount = cashTotal + nonCashTotal,
                DeclaredCashAmount = cashTotal + difference,
                CashDifference = difference,
                Note = difference == 0 ? "Gün sonu kasa kapanışı" : "Sayım farkı kontrol edilecek",
                Status = dayIndex % 4 == 0 ? "Beklemede" : "Onaylandı",
                CreatedAt = closingDay.AddHours(21),
                ReviewedAt = dayIndex % 4 == 0 ? null : closingDay.AddHours(22),
                ReviewNote = dayIndex % 4 == 0 ? null : "Muhasebe kontrolü tamamlandı."
            });
        }

        await context.SaveChangesAsync();
    }

    private static DateTime GetRetailSaleDate(int saleIndex)
    {
        if (saleIndex < 20)
        {
            return DateTime.Today
                .AddHours(8 + saleIndex % 13)
                .AddMinutes(saleIndex * 7 % 60);
        }

        if (saleIndex < 300)
        {
            var dayOffset = 1 + (saleIndex - 20) % 29;
            return DateTime.Today.AddDays(-dayOffset)
                .AddHours(9 + saleIndex % 12)
                .AddMinutes(saleIndex * 7 % 60);
        }

        var historicalOffset = 30 + (saleIndex * 13 % 150);
        return DateTime.Today.AddDays(-historicalOffset)
            .AddHours(9 + saleIndex % 12)
            .AddMinutes(saleIndex * 11 % 60);
    }

    private static decimal GetExpenseAmount(string expenseName, int index)
    {
        return expenseName switch
        {
            "Kira" => 38000m + index % 3 * 1500m,
            "Personel maaşı" => 28000m + index % 5 * 3500m,
            "SGK / sigorta" => 26500m + index % 3 * 2200m,
            "Vergi" => 18000m + index % 5 * 1750m,
            "Tedarikçi ödemesi" => 35000m + index % 5 * 12000m,
            "Elektrik faturası" => 5500m + index % 6 * 2200m,
            "Su faturası" => 1200m + index % 5 * 800m,
            "Doğalgaz faturası" => 3500m + index % 5 * 1200m,
            _ => 850m + index % 10 * 425m
        };
    }

    private static void ValidateBaseDemoData(
        IReadOnlyDictionary<string, Employee> employees,
        IReadOnlyList<Product> products,
        IReadOnlyList<Customer> customers,
        IReadOnlyList<Supplier> suppliers)
    {
        var requiredUsers = new[]
        {
            "admin", "mudur", "kasiyer", "toptanci", "muhasebe", "depocu"
        };
        var missingUsers = requiredUsers.Where(username => !employees.ContainsKey(username)).ToList();

        if (missingUsers.Count > 0 || products.Count == 0 || customers.Count == 0 || suppliers.Count == 0)
        {
            throw new InvalidOperationException(
                "İşlem verileri oluşturulamadı. Önce temel DemoDataSeeder verilerinin " +
                $"oluşturulması gerekir. Eksik kullanıcılar: {string.Join(", ", missingUsers)}");
        }
    }

    private static void NormalizeDemoStockLevels(IReadOnlyList<Product> products)
    {
        for (var index = 0; index < products.Count; index++)
        {
            var product = products[index];
            if (index % 31 == 0)
            {
                product.StockQuantity = 0;
            }
            else if (index % 17 == 0)
            {
                product.StockQuantity = Math.Max(1, product.CriticalStock - 2);
            }
        }
    }

    private static async Task ValidateSeedCountsAsync(AppDbContext context)
    {
        var checks = new Dictionary<string, (int Actual, int Minimum)>
        {
            ["products"] = (await context.Products.CountAsync(), 120),
            ["customers"] = (await context.Customers.CountAsync(), 130),
            ["suppliers"] = (await context.Suppliers.CountAsync(), 20),
            ["sales"] = (await context.Sales.CountAsync(), 300),
            ["sale_details"] = (await context.SaleDetails.CountAsync(), 600),
            ["employee_leaves"] = (await context.EmployeeLeaves.CountAsync(), 50),
            ["employee_shifts"] = (await context.EmployeeShifts.CountAsync(), 300),
            ["expenses"] = (await context.Expenses.CountAsync(), 60),
            ["purchase_orders"] = (await context.PurchaseOrders.CountAsync(), 50),
            ["wholesale_sale_requests"] = (await context.WholesaleSaleRequests.CountAsync(), 50),
            ["return_requests"] = (await context.ReturnRequests.CountAsync(), 40),
            ["cash_register_closings"] = (await context.CashRegisterClosings.CountAsync(), 35),
            ["employee_bonuses"] = (await context.EmployeeBonuses.CountAsync(), 30)
        };

        var failures = checks
            .Where(check => check.Value.Actual < check.Value.Minimum)
            .Select(check =>
                $"{check.Key}: {check.Value.Actual}/{check.Value.Minimum}")
            .ToList();

        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var tomorrow = today.AddDays(1);
        var totalIncome = await context.Sales.SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
        var totalExpense = await context.Expenses.SumAsync(e => (decimal?)e.Amount) ?? 0m;
        var monthlyIncome = await context.Sales
            .Where(s => s.SaleDate >= monthStart && s.SaleDate < tomorrow)
            .SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
        var monthlyExpense = await context.Expenses
            .Where(e => e.ExpenseDate >= monthStart && e.ExpenseDate < tomorrow)
            .SumAsync(e => (decimal?)e.Amount) ?? 0m;

        if (totalIncome < 1_500_000m)
        {
            failures.Add($"total_income: {totalIncome:N2}/1.500.000,00");
        }

        if (totalExpense is < 700_000m or > 1_500_000m)
        {
            failures.Add($"total_expense: {totalExpense:N2}/700.000-1.500.000");
        }

        if (totalIncome - totalExpense < 300_000m)
        {
            failures.Add($"net_profit: {totalIncome - totalExpense:N2}/300.000,00");
        }

        if (monthlyIncome <= monthlyExpense)
        {
            failures.Add(
                $"monthly_net: {monthlyIncome - monthlyExpense:N2} (gelir giderden yüksek olmalı)");
        }

        if (failures.Count > 0)
        {
            throw new InvalidOperationException(
                "Demo veri hedefleri sağlanamadı; transaction geri alınacak. " +
                string.Join(", ", failures));
        }
    }

    private static decimal GetCategoryBasePrice(string category)
    {
        return category switch
        {
            "Meyve & Sebze" => 28m,
            "Ekmek & Unlu Mamuller" => 18m,
            "Süt & Kahvaltılık" => 38m,
            "Temel Gıda" => 34m,
            "Bakliyat & Konserve" => 42m,
            "Yağ & Sos" => 105m,
            "Çay & Kahve" => 78m,
            "Su & İçecek" => 22m,
            "Atıştırmalık" => 34m,
            "Çikolata & Şekerleme" => 38m,
            "Temizlik" => 88m,
            "Kişisel Bakım" => 68m,
            "Kağıt Ürünleri" => 58m,
            "Bebek Ürünleri" => 115m,
            "Ev & Mutfak" => 52m,
            _ => 45m
        };
    }

    private static List<ProductSeed> GetProductCatalog()
    {
        return new Dictionary<string, string[]>
        {
            ["Süt & Kahvaltılık"] =
            [
                "Tam Yağlı Süt 1 L", "Yarım Yağlı Süt 1 L", "Yoğurt 1 Kg",
                "Süzme Yoğurt 750 G", "Beyaz Peynir 500 G", "Kaşar Peyniri 400 G",
                "Tereyağı 250 G", "Yumurta 15'li", "Kefir 1 L", "Labne Peynir 200 G"
            ],
            ["Ekmek & Unlu Mamuller"] =
            [
                "Günlük Ekmek", "Tam Buğday Ekmek", "Tost Ekmeği",
                "Lavaş 6'lı", "Hamburger Ekmeği 4'lü", "Galeta 250 G",
                "Simit", "Sandviç Ekmeği 5'li"
            ],
            ["Temel Gıda"] =
            [
                "Spagetti Makarna 500 G", "Burgu Makarna 500 G", "Pirinç 1 Kg",
                "Pilavlık Bulgur 1 Kg", "Köftelik Bulgur 1 Kg", "Un 2 Kg",
                "Toz Şeker 1 Kg", "İrmik 500 G", "Mısır Unu 1 Kg", "Tuz 750 G",
                "Küp Şeker 1 Kg", "Erişte 500 G"
            ],
            ["Bakliyat & Konserve"] =
            [
                "Kırmızı Mercimek 1 Kg", "Yeşil Mercimek 1 Kg", "Nohut 1 Kg",
                "Kuru Fasulye 1 Kg", "Barbunya 800 G", "Konserve Mısır 400 G",
                "Ton Balığı 2x160 G", "Bezelye Konservesi 800 G",
                "Haşlanmış Nohut Konservesi", "Domates Konservesi 800 G"
            ],
            ["Yağ & Sos"] =
            [
                "Ayçiçek Yağı 1 L", "Ayçiçek Yağı 2 L", "Zeytinyağı 1 L",
                "Domates Salçası 830 G", "Ketçap 600 G", "Mayonez 500 G",
                "Nar Ekşisi 250 Ml", "Üzüm Sirkesi 1 L"
            ],
            ["Çay & Kahve"] =
            [
                "Siyah Çay 500 G", "Siyah Çay 1 Kg", "Türk Kahvesi 100 G",
                "Filtre Kahve 250 G", "Granül Kahve 100 G", "Bitki Çayı 20'li",
                "Kakao 100 G", "Espresso Kahve 250 G", "Ihlamur 50 G"
            ],
            ["Su & İçecek"] =
            [
                "Su 500 Ml", "Su 1.5 L", "Maden Suyu 6'lı", "Portakal Suyu 1 L",
                "Vişne Nektarı 1 L", "Kola 1 L", "Kola 2.5 L", "Gazoz 1 L",
                "Ayran 1 L", "Şalgam Suyu 1 L", "Limonata 1 L", "Soğuk Çay 1 L"
            ],
            ["Atıştırmalık"] =
            [
                "Patates Cipsi Klasik", "Patates Cipsi Baharatlı", "Mısır Cipsi",
                "Tuzlu Kraker", "Çubuk Kraker", "Yulaflı Bisküvi",
                "Kremalı Bisküvi", "Karışık Kuruyemiş 200 G",
                "Patlamış Mısır 100 G", "Protein Bar 45 G"
            ],
            ["Çikolata & Şekerleme"] =
            [
                "Sütlü Çikolata 60 G", "Bitter Çikolata 60 G", "Fındıklı Çikolata",
                "Gofret 40 G", "Jelibon 80 G", "Sakız 5'li", "Lokum 250 G",
                "Draje Çikolata 80 G", "Karamelli Bar 45 G"
            ],
            ["Meyve & Sebze"] =
            [
                "Domates 1 Kg", "Salatalık 1 Kg", "Patates 1 Kg", "Kuru Soğan 1 Kg",
                "Elma 1 Kg", "Muz 1 Kg", "Portakal 1 Kg", "Limon 500 G",
                "Havuç 1 Kg", "Biber 500 G", "Armut 1 Kg", "Kabak 1 Kg"
            ],
            ["Temizlik"] =
            [
                "Çamaşır Deterjanı 3 Kg", "Bulaşık Deterjanı 750 Ml",
                "Yüzey Temizleyici 1 L", "Çamaşır Suyu 2 L", "Yumuşatıcı 1.5 L",
                "Cam Temizleyici 500 Ml", "Bulaşık Makinesi Tableti 30'lu",
                "Temizlik Süngeri 5'li", "Yağ Çözücü 750 Ml", "Mop Temizleyici 1 L"
            ],
            ["Kişisel Bakım"] =
            [
                "Şampuan 400 Ml", "Duş Jeli 500 Ml", "Sıvı Sabun 500 Ml",
                "Diş Macunu 100 Ml", "Diş Fırçası Orta", "Deodorant 150 Ml",
                "El Kremi 75 Ml", "Tıraş Köpüğü 200 Ml"
            ],
            ["Kağıt Ürünleri"] =
            [
                "Tuvalet Kağıdı 12'li", "Kağıt Havlu 6'lı",
                "Peçete 100'lü", "Islak Mendil 90'lı",
                "Mendil 10'lu Paket", "Pişirme Kağıdı 16 Yaprak"
            ],
            ["Bebek Ürünleri"] =
            [
                "Bebek Bezi 30'lu", "Bebek Şampuanı 500 Ml", "Bebek Maması 400 G",
                "Bebek Islak Mendili 72'li", "Bebek Yağı 300 Ml"
            ],
            ["Ev & Mutfak"] =
            [
                "Çöp Torbası Orta Boy", "Alüminyum Folyo 15 M",
                "Streç Film 30 M", "Buzdolabı Poşeti 20'li",
                "Kilitli Saklama Poşeti 15'li", "Kağıt Bardak 50'li"
            ]
        }
        .SelectMany(group => group.Value.Select(name => new ProductSeed(name, group.Key)))
        .ToList();
    }

    private sealed record EmployeeSeed(
        string FullName,
        string Username,
        string RoleName,
        decimal Salary,
        int HireYearOffset);

    private sealed record CategorySeed(string Name, decimal VatRate);

    private sealed record ProductSeed(string Name, string Category);
}
