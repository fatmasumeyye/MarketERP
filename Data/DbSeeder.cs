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
                new Permission { Code = "sql.editor", Description = "SQL rapor panelini kullanma" }
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
                "support.manage"
            );

            await AssignPermissionsToRoleAsync(context, "Kasiyer",
                 "dashboard.view",
                 "sale.retail.create",
                 "sale.view.own"
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
                "reports.sales"
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
                "supplier.view"
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