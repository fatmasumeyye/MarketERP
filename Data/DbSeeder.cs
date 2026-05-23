using MarketERP.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketERP.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            await context.Database.MigrateAsync();

            if (!context.Roles.Any())
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

                context.Roles.AddRange(roles);
                await context.SaveChangesAsync();
            }

            if (!context.Permissions.Any())
            {
                var permissions = new List<Permission>
                {
                    new Permission { Code = "dashboard.view", Description = "Dashboard görüntüleme" },

                    new Permission { Code = "product.view", Description = "Ürün görüntüleme" },
                    new Permission { Code = "product.create", Description = "Ürün ekleme" },
                    new Permission { Code = "product.update", Description = "Ürün güncelleme" },
                    new Permission { Code = "product.delete", Description = "Ürün silme" },
                    new Permission { Code = "product.changePrice", Description = "Ürün fiyatı değiştirme" },

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

                    new Permission { Code = "finance.view", Description = "Finans görüntüleme" },
                    new Permission { Code = "payment.collect", Description = "Tahsilat alma" },
                    new Permission { Code = "payment.refund", Description = "Ödeme iadesi" },

                    new Permission { Code = "user.manage", Description = "Kullanıcı yönetimi" },
                    new Permission { Code = "role.manage", Description = "Rol ve yetki yönetimi" },

                    new Permission { Code = "reports.sales", Description = "Satış raporları" },
                    new Permission { Code = "reports.stock", Description = "Stok raporları" },
                    new Permission { Code = "reports.financial", Description = "Finansal raporlar" },
                    new Permission { Code = "reports.employee", Description = "Personel raporları" },

                    new Permission { Code = "support.view", Description = "Destek taleplerini görme" },
                    new Permission { Code = "support.manage", Description = "Destek taleplerini yönetme" }
                };

                context.Permissions.AddRange(permissions);
                await context.SaveChangesAsync();
            }

            if (!context.Employees.Any(e => e.Username == "admin"))
            {
                var admin = new Employee
                {
                    FullName = "Sistem Yöneticisi",
                    Phone = "-",
                    Position = "Admin",
                    Email = "admin@marketerp.com",
                    Username = "admin",
                    Password = "123456",
                    IsActive = true,
                    HireDate = DateTime.Now
                };

                context.Employees.Add(admin);
                await context.SaveChangesAsync();

                var adminRole = context.Roles.First(r => r.Name == "Admin");

                context.UserRoles.Add(new UserRole
                {
                    EmployeeId = admin.Id,
                    RoleId = adminRole.Id
                });

                await context.SaveChangesAsync();
            }

            var adminRoleForPermissions = context.Roles.FirstOrDefault(r => r.Name == "Admin");

            if (adminRoleForPermissions != null && !context.RolePermissions.Any(rp => rp.RoleId == adminRoleForPermissions.Id))
            {
                var allPermissions = context.Permissions.ToList();

                foreach (var permission in allPermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = adminRoleForPermissions.Id,
                        PermissionId = permission.Id
                    });
                }

                await context.SaveChangesAsync();
            }
        }
    }
}