using Microsoft.AspNetCore.Identity;
using MarketERP.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketERP.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            await context.Database.MigrateAsync();

            await EnsureRolesAsync(context);
            await EnsurePermissionsAsync(context);
            await EnsureCategoryTreeAsync(context);
            await EnsureSavedQueriesAsync(context);
            await EnsureRolePermissionsAsync(context);
            await EnsureDefaultUsersAsync(context);
        }

        private static async Task EnsureRolesAsync(AppDbContext context)
        {
            var roles = new List<Role>
            {
                new Role { Name = "Admin", Description = "Sistem yöneticisi" },
                new Role { Name = "İşletme Sahibi", Description = "Tüm işletme raporlarını ve süreçlerini görür" },
                new Role { Name = "Mağaza Müdürü", Description = "Şube ve personel yönetimi yapar" },
                new Role { Name = "Kasiyer", Description = "Perakende satış yapar" },
                new Role { Name = "Toptan Satış Sorumlusu", Description = "Toptan satış ve teklif süreçlerini yönetir" },
                new Role { Name = "Depo Sorumlusu", Description = "Stok ve depo işlemlerini yönetir" },
                new Role { Name = "Satın Alma Sorumlusu", Description = "Tedarikçi ve satın alma süreçlerini yönetir" },
                new Role { Name = "Muhasebe", Description = "Finans, ödeme ve tahsilat işlemlerini yönetir" },
                new Role { Name = "Müşteri Hizmetleri", Description = "Müşteri talep ve destek süreçlerini yönetir" }
            };

            foreach (var role in roles)
            {
                if (!await context.Roles.AnyAsync(r => r.Name == role.Name))
                {
                    context.Roles.Add(role);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsurePermissionsAsync(AppDbContext context)
        {
            var permissions = new List<Permission>
            {
                new Permission { Code = "dashboard.view", Description = "Dashboard görüntüleme" },

                new Permission { Code = "product.view", Description = "Ürün görüntüleme" },
                new Permission { Code = "product.create", Description = "Ürün ekleme" },
                new Permission { Code = "product.update", Description = "Ürün güncelleme" },
                new Permission { Code = "product.delete", Description = "Ürün silme" },
                new Permission { Code = "product.changePrice", Description = "Ürün fiyatı değiştirme" },

                new Permission { Code = "category.view", Description = "Kategori görüntüleme" },
                new Permission { Code = "category.manage", Description = "Kategori yönetimi" },

                new Permission { Code = "customer.view", Description = "Müşteri görüntüleme" },
                new Permission { Code = "customer.create", Description = "Müşteri ekleme" },
                new Permission { Code = "customer.update", Description = "Müşteri güncelleme" },

                new Permission { Code = "sale.retail.create", Description = "Perakende satış yapma" },
                new Permission { Code = "sale.wholesale.create", Description = "Toptan satış yapma" },
                new Permission { Code = "sale.view.own", Description = "Kendi satışlarını görme" },
                new Permission { Code = "sale.view.branch", Description = "Şube satışlarını görme" },
                new Permission { Code = "sale.view.all", Description = "Tüm satışları görme" },
                new Permission { Code = "sale.cancel", Description = "Satış iptal etme" },
                new Permission { Code = "sale.refund", Description = "İade işlemi yapma" },
                new Permission { Code = "sale.discount", Description = "İndirim uygulama" },

                new Permission { Code = "stock.view", Description = "Stok görüntüleme" },
                new Permission { Code = "stock.adjust", Description = "Stok düzeltme" },
                new Permission { Code = "stock.transfer", Description = "Depolar arası transfer" },
                new Permission { Code = "stock.count", Description = "Stok sayımı" },

                new Permission { Code = "purchase.view", Description = "Satın alma görüntüleme" },
                new Permission { Code = "purchase.create", Description = "Satın alma oluşturma" },
                new Permission { Code = "purchase.approve", Description = "Satın alma onaylama" },

                new Permission { Code = "supplier.view", Description = "Tedarikçi görüntüleme" },
                new Permission { Code = "supplier.manage", Description = "Tedarikçi yönetimi" },

                new Permission { Code = "finance.view", Description = "Finans görüntüleme" },
                new Permission { Code = "payment.collect", Description = "Tahsilat alma" },
                new Permission { Code = "payment.refund", Description = "Ödeme iadesi" },

                new Permission { Code = "employee.view", Description = "Personel görüntüleme" },
                new Permission { Code = "employee.manage", Description = "Personel yönetimi" },

                new Permission { Code = "user.manage", Description = "Kullanıcı yönetimi" },
                new Permission { Code = "role.manage", Description = "Rol ve yetki yönetimi" },

                new Permission { Code = "reports.sales", Description = "Satış raporları" },
                new Permission { Code = "reports.stock", Description = "Stok raporları" },
                new Permission { Code = "reports.financial", Description = "Finansal raporlar" },
                new Permission { Code = "reports.employee", Description = "Personel raporları" },

                new Permission { Code = "support.view", Description = "Destek taleplerini görme" },
                new Permission { Code = "support.manage", Description = "Destek taleplerini yönetme" },

                new Permission { Code = "database.view", Description = "Veritabanı şemasını görüntüleme" },
                new Permission { Code = "sql.editor", Description = "SQL rapor panelini kullanma" },

                new Permission { Code = "return.request", Description = "İade talebi oluşturma" },
                new Permission { Code = "return.approve", Description = "İade talebi onaylama" },

                new Permission { Code = "cash.closing.create", Description = "Kasa kapanışı oluşturma" },
                new Permission { Code = "cash.closing.approve", Description = "Kasa kapanışı onaylama" },

                new Permission { Code = "leave.request.create", Description = "İzin talebi oluşturma" },
                new Permission { Code = "leave.request.view", Description = "İzin taleplerini görüntüleme" },
                new Permission { Code = "leave.request.approve", Description = "İzin taleplerini onaylama ve reddetme" }
            };

            foreach (var permission in permissions)
            {
                if (!await context.Permissions.AnyAsync(p => p.Code == permission.Code))
                {
                    context.Permissions.Add(permission);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureCategoryTreeAsync(AppDbContext context)
        {
            var food = await EnsureMainCategoryAsync(context, "Gıda");
            await EnsureSubCategoryAsync(context, food, "Et & Tavuk", 1);
            await EnsureSubCategoryAsync(context, food, "Balık & Deniz Ürünleri", 1);
            await EnsureSubCategoryAsync(context, food, "Süt & Süt Ürünleri", 1);
            await EnsureSubCategoryAsync(context, food, "Yumurta", 1);
            await EnsureSubCategoryAsync(context, food, "Meyve & Sebze", 1);
            await EnsureSubCategoryAsync(context, food, "Tahıl & Bakliyat", 1);
            await EnsureSubCategoryAsync(context, food, "Un / Pirinç / Bulgur / Makarna", 1);
            await EnsureSubCategoryAsync(context, food, "Ekmek & Unlu Mamuller", 1);
            await EnsureSubCategoryAsync(context, food, "Yağ & Zeytin", 1);
            await EnsureSubCategoryAsync(context, food, "Çay / Kahve / Baharat", 1);
            await EnsureSubCategoryAsync(context, food, "Şekerleme & Çikolata", 1);
            await EnsureSubCategoryAsync(context, food, "Konserve / Reçel / Salça / Sos", 1);

            var drinks = await EnsureMainCategoryAsync(context, "İçecek");
            await EnsureSubCategoryAsync(context, drinks, "Su & Sade Maden Suyu", 1);
            await EnsureSubCategoryAsync(context, drinks, "%100 Meyve / Sebze Suyu", 1);
            await EnsureSubCategoryAsync(context, drinks, "Şalgam / Sirke / Üzüm Şırası / Meyveli Soda", 1);
            await EnsureSubCategoryAsync(context, drinks, "Meyve Nektarı & Meyveli İçecek", 10);
            await EnsureSubCategoryAsync(context, drinks, "Gazoz / Kola / Alkolsüz Bira", 10);

            var readyConsumption = await EnsureMainCategoryAsync(context, "Hazır Tüketim Hizmeti");
            await EnsureSubCategoryAsync(context, readyConsumption, "Market İçi Yeme-İçme Hizmeti", 10);

            var personalCare = await EnsureMainCategoryAsync(context, "Kişisel Bakım & Hijyen");
            await EnsureSubCategoryAsync(context, personalCare, "Diş Bakım Ürünleri", 10);
            await EnsureSubCategoryAsync(context, personalCare, "Hijyenik Ped / Bebek Bezi / 9619 Ürünleri", 10);
            await EnsureSubCategoryAsync(context, personalCare, "Bebek Maması / İnsan Gıdası Maması", 10);

            var cleaning = await EnsureMainCategoryAsync(context, "Temizlik");
            await EnsureSubCategoryAsync(context, cleaning, "Sabun / Şampuan / Dezenfektan / Islak Mendil", 20);
            await EnsureSubCategoryAsync(context, cleaning, "Deterjan & Yüzey Temizlik", 20);
            await EnsureSubCategoryAsync(context, cleaning, "Kağıt Hijyen Ürünleri", 20);
            await EnsureSubCategoryAsync(context, cleaning, "Kozmetik & Makyaj", 20);

            var other = await EnsureMainCategoryAsync(context, "Diğer Market Ürünleri");
            await EnsureSubCategoryAsync(context, other, "Diğer Ürünler", 20);

            await context.SaveChangesAsync();
        }

        public static async Task EnsureSavedQueriesAsync(AppDbContext context)
        {
            var preparedQueries = new Dictionary<string, string>
            {
                ["Bugünkü Satışlar"] = """
                    SELECT s.id, s.sale_date, c.full_name AS customer_name,
                           e.full_name AS employee_name, s.payment_type, s.total_amount
                    FROM sales s
                    LEFT JOIN customers c ON c.id = s.customer_id
                    LEFT JOIN employees e ON e.id = s.employee_id
                    WHERE s.sale_date >= CURDATE()
                      AND s.sale_date < CURDATE() + INTERVAL 1 DAY
                    ORDER BY s.sale_date DESC;
                    """,
                ["Bugünkü Ciro"] = """
                    SELECT COUNT(*) AS sale_count,
                           COALESCE(SUM(total_amount), 0) AS total_revenue
                    FROM sales
                    WHERE sale_date >= CURDATE()
                      AND sale_date < CURDATE() + INTERVAL 1 DAY;
                    """,
                ["Aylık Satış Özeti"] = """
                    SELECT DATE_FORMAT(sale_date, '%Y-%m') AS sale_month,
                           COUNT(*) AS sale_count,
                           COALESCE(SUM(total_amount), 0) AS total_revenue,
                           COALESCE(AVG(total_amount), 0) AS average_basket
                    FROM sales
                    GROUP BY DATE_FORMAT(sale_date, '%Y-%m')
                    ORDER BY sale_month DESC;
                    """,
                ["En Çok Satan Ürünler"] = """
                    SELECT p.id, p.name,
                           SUM(sd.quantity) AS total_quantity,
                           SUM(sd.subtotal) AS total_revenue
                    FROM sale_details sd
                    INNER JOIN products p ON p.id = sd.product_id
                    GROUP BY p.id, p.name
                    ORDER BY total_quantity DESC
                    LIMIT 20;
                    """,
                ["Kritik Stoktaki Ürünler"] = """
                    SELECT p.id, p.name, p.stock_quantity, p.critical_stock
                    FROM products p
                    WHERE p.stock_quantity > 0
                      AND p.stock_quantity <= p.critical_stock
                    ORDER BY p.stock_quantity, p.name;
                    """,
                ["Stokta Biten Ürünler"] = """
                    SELECT p.id, p.name, p.stock_quantity, p.critical_stock
                    FROM products p
                    WHERE p.stock_quantity <= 0
                    ORDER BY p.name;
                    """,
                ["Kategori Bazlı Ürün Sayısı"] = """
                    SELECT c.id, c.name AS category_name,
                           COUNT(p.id) AS product_count
                    FROM categories c
                    LEFT JOIN products p ON p.category_id = c.id
                    GROUP BY c.id, c.name
                    ORDER BY product_count DESC, c.name;
                    """,
                ["Kategori Bazlı Satış Toplamı"] = """
                    SELECT c.id, c.name AS category_name,
                           COALESCE(SUM(sd.quantity), 0) AS sold_quantity,
                           COALESCE(SUM(sd.subtotal), 0) AS total_revenue
                    FROM categories c
                    LEFT JOIN products p ON p.category_id = c.id
                    LEFT JOIN sale_details sd ON sd.product_id = p.id
                    GROUP BY c.id, c.name
                    ORDER BY total_revenue DESC, c.name;
                    """,
                ["Müşteri Bazlı Satış Toplamı"] = """
                    SELECT c.id, c.full_name,
                           COUNT(s.id) AS sale_count,
                           COALESCE(SUM(s.total_amount), 0) AS total_amount
                    FROM customers c
                    LEFT JOIN sales s ON s.customer_id = c.id
                    GROUP BY c.id, c.full_name
                    ORDER BY c.full_name;
                    """,
                ["En Çok Alışveriş Yapan Müşteriler"] = """
                    SELECT c.id, c.full_name,
                           COUNT(s.id) AS sale_count,
                           SUM(s.total_amount) AS total_amount
                    FROM sales s
                    INNER JOIN customers c ON c.id = s.customer_id
                    GROUP BY c.id, c.full_name
                    ORDER BY total_amount DESC
                    LIMIT 20;
                    """,
                ["Bekleyen İade Talepleri"] = """
                    SELECT rr.request_no, rr.sale_id, e.full_name AS employee_name,
                           MIN(rr.requested_at) AS requested_at,
                           SUM(rr.quantity) AS total_quantity
                    FROM return_requests rr
                    LEFT JOIN employees e ON e.id = rr.employee_id
                    WHERE rr.status IN ('Beklemede', 'Onay Bekliyor')
                    GROUP BY rr.request_no, rr.sale_id, e.full_name
                    ORDER BY requested_at DESC;
                    """,
                ["Onaylanan İade Talepleri"] = """
                    SELECT rr.request_no, rr.sale_id, e.full_name AS employee_name,
                           MIN(rr.requested_at) AS requested_at,
                           MAX(rr.reviewed_at) AS reviewed_at,
                           SUM(rr.quantity) AS total_quantity
                    FROM return_requests rr
                    LEFT JOIN employees e ON e.id = rr.employee_id
                    WHERE rr.status = 'Onaylandı'
                    GROUP BY rr.request_no, rr.sale_id, e.full_name
                    ORDER BY reviewed_at DESC;
                    """,
                ["Bekleyen Toptan Satış Talepleri"] = """
                    SELECT w.id, w.request_date, c.full_name AS customer_name,
                           e.full_name AS employee_name, w.total_amount, w.status
                    FROM wholesale_sale_requests w
                    INNER JOIN customers c ON c.id = w.customer_id
                    LEFT JOIN employees e ON e.id = w.employee_id
                    WHERE w.status IN ('Beklemede', 'Onay Bekliyor')
                    ORDER BY w.request_date DESC;
                    """,
                ["Onaylanan Toptan Satış Talepleri"] = """
                    SELECT w.id, w.request_date, w.approved_at,
                           c.full_name AS customer_name,
                           e.full_name AS employee_name, w.total_amount
                    FROM wholesale_sale_requests w
                    INNER JOIN customers c ON c.id = w.customer_id
                    LEFT JOIN employees e ON e.id = w.employee_id
                    WHERE w.status = 'Onaylandı'
                    ORDER BY w.approved_at DESC;
                    """,
                ["Bekleyen Kasa Kapanışları"] = """
                    SELECT crc.id, crc.closing_date, e.full_name AS employee_name,
                           crc.total_sales_amount, crc.declared_cash_amount,
                           crc.cash_difference, crc.status
                    FROM cash_register_closings crc
                    INNER JOIN employees e ON e.id = crc.employee_id
                    WHERE crc.status = 'Beklemede'
                    ORDER BY crc.closing_date DESC;
                    """,
                ["Kasa Kapanış Toplamları"] = """
                    SELECT DATE(closing_date) AS closing_day,
                           COUNT(*) AS closing_count,
                           SUM(cash_sales_total) AS cash_sales_total,
                           SUM(card_sales_total) AS card_sales_total,
                           SUM(total_sales_amount) AS total_sales_amount,
                           SUM(cash_difference) AS total_cash_difference
                    FROM cash_register_closings
                    GROUP BY DATE(closing_date)
                    ORDER BY closing_day DESC;
                    """,
                ["Bekleyen İzin Talepleri"] = """
                    SELECT el.id, e.full_name, e.position, el.start_date,
                           el.end_date, el.leave_reason, el.status
                    FROM employee_leaves el
                    INNER JOIN employees e ON e.id = el.employee_id
                    WHERE el.status IN ('Beklemede', 'Onay Bekliyor')
                    ORDER BY el.start_date;
                    """,
                ["Personel Bazlı İzin Kullanımı"] = """
                    SELECT e.id, e.full_name, e.position,
                           COUNT(el.id) AS approved_leave_count,
                           COALESCE(SUM(DATEDIFF(el.end_date, el.start_date) + 1), 0) AS used_leave_days
                    FROM employees e
                    LEFT JOIN employee_leaves el
                      ON el.employee_id = e.id
                     AND el.status = 'Onaylandı'
                    GROUP BY e.id, e.full_name, e.position
                    ORDER BY used_leave_days DESC, e.full_name;
                    """,
                ["Tedarikçi Bazlı Ürün Sayısı"] = """
                    SELECT s.id, s.company_name,
                           COUNT(p.id) AS product_count
                    FROM suppliers s
                    LEFT JOIN products p ON p.supplier_id = s.id
                    GROUP BY s.id, s.company_name
                    ORDER BY product_count DESC, s.company_name;
                    """,
                ["Satın Alma Sipariş Durumu"] = """
                    SELECT po.status,
                           COUNT(*) AS order_count,
                           COALESCE(SUM(po.total_amount), 0) AS total_amount
                    FROM purchase_orders po
                    GROUP BY po.status
                    ORDER BY order_count DESC;
                    """,
                ["Düşük Stok ve Tedarikçi Listesi"] = """
                    SELECT p.id, p.name, p.stock_quantity, p.critical_stock,
                           s.company_name AS supplier_name, s.phone, s.email
                    FROM products p
                    LEFT JOIN suppliers s ON s.id = p.supplier_id
                    WHERE p.stock_quantity <= p.critical_stock
                    ORDER BY p.stock_quantity, p.name;
                    """,
                ["Ödeme Tipi Bazlı Satış Dağılımı"] = """
                    SELECT COALESCE(NULLIF(payment_type, ''), 'Belirtilmedi') AS payment_type,
                           COUNT(*) AS sale_count,
                           SUM(total_amount) AS total_amount
                    FROM sales
                    GROUP BY COALESCE(NULLIF(payment_type, ''), 'Belirtilmedi')
                    ORDER BY total_amount DESC;
                    """,
                ["Günlük Satış Adet ve Tutar Grafiği"] = """
                    SELECT DATE(sale_date) AS sale_day,
                           COUNT(*) AS sale_count,
                           SUM(total_amount) AS total_amount
                    FROM sales
                    WHERE sale_date >= CURDATE() - INTERVAL 29 DAY
                    GROUP BY DATE(sale_date)
                    ORDER BY sale_day;
                    """,
                ["İade Oranı Raporu"] = """
                    SELECT COUNT(DISTINCT rr.request_no) AS return_request_count,
                           COUNT(DISTINCT rr.sale_id) AS returned_sale_count,
                           (SELECT COUNT(*) FROM sales) AS total_sale_count,
                           ROUND(
                               COUNT(DISTINCT rr.sale_id) * 100.0
                               / NULLIF((SELECT COUNT(*) FROM sales), 0),
                               2
                           ) AS return_rate_percent
                    FROM return_requests rr;
                    """,
                ["Ürün Bazlı Kâr Tahmini"] = """
                    SELECT p.id, p.name,
                           SUM(sd.quantity) AS sold_quantity,
                           SUM(sd.subtotal) AS sales_revenue,
                           SUM(sd.quantity * p.purchase_price) AS estimated_cost,
                           SUM(sd.subtotal - (sd.quantity * p.purchase_price)) AS estimated_profit
                    FROM sale_details sd
                    INNER JOIN products p ON p.id = sd.product_id
                    GROUP BY p.id, p.name
                    ORDER BY estimated_profit DESC;
                    """
            };

            var existingQueries = await context.SavedQueries.ToListAsync();

            foreach (var preparedQuery in preparedQueries)
            {
                var existingQuery = existingQueries.FirstOrDefault(q =>
                    string.Equals(
                        q.Title?.Trim(),
                        preparedQuery.Key,
                        StringComparison.OrdinalIgnoreCase));

                if (existingQuery == null)
                {
                    context.SavedQueries.Add(new SavedQuery
                    {
                        Title = preparedQuery.Key,
                        SqlQuery = preparedQuery.Value.Trim()
                    });
                }
                else
                {
                    existingQuery.SqlQuery = preparedQuery.Value.Trim();
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task<Category> EnsureMainCategoryAsync(AppDbContext context, string name)
        {
            var category = await context.Categories
                .FirstOrDefaultAsync(c => c.Name == name && c.ParentCategoryId == null);

            if (category == null)
            {
                category = new Category
                {
                    Name = name,
                    ParentCategoryId = null,
                    DefaultVatRate = null
                };

                context.Categories.Add(category);
                await context.SaveChangesAsync();
            }

            return category;
        }

        private static async Task EnsureSubCategoryAsync(
            AppDbContext context,
            Category parentCategory,
            string name,
            decimal defaultVatRate)
        {
            var category = await context.Categories
                .FirstOrDefaultAsync(c =>
                    c.Name == name &&
                    c.ParentCategoryId == parentCategory.Id);

            if (category == null)
            {
                category = new Category
                {
                    Name = name,
                    ParentCategoryId = parentCategory.Id,
                    DefaultVatRate = defaultVatRate
                };

                context.Categories.Add(category);
            }
            else
            {
                category.DefaultVatRate = defaultVatRate;
            }
        }

        private static async Task EnsureRolePermissionsAsync(AppDbContext context)
        {
            await AssignPermissionsToRoleAsync(context, "Admin", "ALL");

            await AssignPermissionsToRoleAsync(context, "İşletme Sahibi",
                "dashboard.view",
                "product.view",
                "category.view",
                "customer.view",
                "sale.view.all",
                "stock.view",
                "purchase.view",
                "supplier.view",
                "finance.view",
                "reports.sales",
                "reports.stock",
                "reports.financial",
                "reports.employee",
                "employee.view",
                "support.view"
            );

            await AssignPermissionsToRoleAsync(context, "Mağaza Müdürü",
                "dashboard.view",
                "product.view",
                "category.view",
                "customer.view",
                "customer.create",
                "customer.update",
                "sale.retail.create",
                "sale.view.branch",
                "sale.cancel",
                "sale.refund",
                "sale.discount",
                "stock.view",
                "stock.adjust",
                "reports.sales",
                "reports.stock",
                "employee.view",
                "support.view",
                "support.manage",
                "return.approve",
                "cash.closing.approve",
                "leave.request.view",
                "leave.request.approve"
            );

            await AssignPermissionsToRoleAsync(context, "Kasiyer",
                "dashboard.view",
                "sale.retail.create",
                "sale.view.own",
                "return.request",
                "cash.closing.create",
                "leave.request.create"
            );

            await AssignPermissionsToRoleAsync(context, "Toptan Satış Sorumlusu",
                "dashboard.view",
                "product.view",
                "category.view",
                "customer.view",
                "customer.create",
                "customer.update",
                "sale.wholesale.create",
                "sale.view.own",
                "sale.discount",
                "stock.view",
                "reports.sales",
                "leave.request.create"
            );

            await AssignPermissionsToRoleAsync(context, "Depo Sorumlusu",
                "dashboard.view",
                "product.view",
                "product.create",
                "product.update",
                "category.view",
                "stock.view",
                "stock.adjust",
                "stock.transfer",
                "stock.count",
                "reports.stock",
                "supplier.view",
                "leave.request.create"
            );

            await AssignPermissionsToRoleAsync(context, "Satın Alma Sorumlusu",
                "dashboard.view",
                "product.view",
                "product.create",
                "product.update",
                "category.view",
                "purchase.view",
                "purchase.create",
                "supplier.view",
                "supplier.manage",
                "stock.view"
            );

            await AssignPermissionsToRoleAsync(context, "Muhasebe",
                "dashboard.view",
                "finance.view",
                "payment.collect",
                "payment.refund",
                "sale.view.all",
                "purchase.view",
                "reports.sales",
                "reports.financial"
            );

            await AssignPermissionsToRoleAsync(context, "Müşteri Hizmetleri",
                "dashboard.view",
                "customer.view",
                "customer.create",
                "customer.update",
                "support.view",
                "support.manage",
                "sale.view.own"
            );
        }

        private static async Task AssignPermissionsToRoleAsync(AppDbContext context, string roleName, params string[] permissionCodes)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);

            if (role == null)
                return;

            List<Permission> permissions;

            if (permissionCodes.Length == 1 && permissionCodes[0] == "ALL")
            {
                permissions = await context.Permissions.ToListAsync();
            }
            else
            {
                permissions = await context.Permissions
                    .Where(p => permissionCodes.Contains(p.Code))
                    .ToListAsync();
            }

            foreach (var permission in permissions)
            {
                var exists = await context.RolePermissions.AnyAsync(rp =>
                    rp.RoleId == role.Id &&
                    rp.PermissionId == permission.Id);

                if (!exists)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.Id
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureDefaultUsersAsync(AppDbContext context)
        {
            await EnsureUserAsync(context,
                fullName: "Sistem Yöneticisi",
                username: "admin",
                password: "123456",
                roleName: "Admin",
                position: "Admin",
                email: "admin@marketerp.com"
            );

            await EnsureUserAsync(context,
                fullName: "Test Kasiyer",
                username: "kasiyer",
                password: "123456",
                roleName: "Kasiyer",
                position: "Kasiyer",
                email: "kasiyer@marketerp.com"
            );

            await EnsureUserAsync(context,
                fullName: "Test Depo Sorumlusu",
                username: "depocu",
                password: "123456",
                roleName: "Depo Sorumlusu",
                position: "Depo Sorumlusu",
                email: "depocu@marketerp.com"
            );

            await EnsureUserAsync(context,
                fullName: "Test Muhasebe",
                username: "muhasebe",
                password: "123456",
                roleName: "Muhasebe",
                position: "Muhasebe",
                email: "muhasebe@marketerp.com"
            );

            await EnsureUserAsync(context,
                fullName: "Test Toptan Satış Sorumlusu",
                username: "toptanci",
                password: "123456",
                roleName: "Toptan Satış Sorumlusu",
                position: "Toptan Satış Sorumlusu",
                email: "toptanci@marketerp.com"
            );

            await EnsureUserAsync(context,
                fullName: "Test Müşteri Hizmetleri",
                username: "musteri",
                password: "123456",
                roleName: "Müşteri Hizmetleri",
                position: "Müşteri Hizmetleri",
                email: "musteri@marketerp.com"
            );
        }

        private static async Task EnsureUserAsync(
            AppDbContext context,
            string fullName,
            string username,
            string password,
            string roleName,
            string position,
            string email)
        {
            var user = await context.Employees.FirstOrDefaultAsync(e => e.Username == username);

            if (user == null)
            {
                var passwordHasher = new PasswordHasher<Employee>();

                user = new Employee
                {
                    FullName = fullName,
                    Phone = "-",
                    Position = position,
                    Email = email,
                    Username = username,
                    IsActive = true,
                    HireDate = DateTime.Now
                };

                user.Password = passwordHasher.HashPassword(user, password);

                context.Employees.Add(user);
                await context.SaveChangesAsync();
            }

            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);

            if (role == null)
                return;

            var hasRole = await context.UserRoles.AnyAsync(ur =>
                ur.EmployeeId == user.Id &&
                ur.RoleId == role.Id);

            if (!hasRole)
            {
                context.UserRoles.Add(new UserRole
                {
                    EmployeeId = user.Id,
                    RoleId = role.Id
                });

                await context.SaveChangesAsync();
            }
        }
    }
}
