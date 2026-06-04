using Microsoft.EntityFrameworkCore;
using MarketERP.Models;

namespace MarketERP.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeBonus> EmployeeBonuses { get; set; }
        public DbSet<EmployeeLeave> EmployeeLeaves { get; set; }
        public DbSet<EmployeeShift> EmployeeShifts { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SavedQuery> SavedQueries { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<ReturnRequest> ReturnRequests { get; set; }
        public DbSet<CashRegisterClosing> CashRegisterClosings { get; set; }
    }
}