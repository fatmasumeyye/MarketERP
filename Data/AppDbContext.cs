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

        public DbSet<ProductSupplier> ProductSuppliers { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<WholesaleSaleRequest> WholesaleSaleRequests { get; set; }
        public DbSet<WholesaleSaleRequestItem> WholesaleSaleRequestItems { get; set; }
        public DbSet<ProjectModule> ProjectModules { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<ProjectTeamMember> ProjectTeamMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .Property(c => c.DefaultVatRate)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.PurchasePrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.SalePrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductSupplier>()
                .Property(ps => ps.PurchasePrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductSupplier>()
                .HasOne(ps => ps.Product)
                .WithMany(p => p.ProductSuppliers)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductSupplier>()
                .HasOne(ps => ps.Supplier)
                .WithMany(s => s.ProductSuppliers)
                .HasForeignKey(ps => ps.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrder>()
                .Property(po => po.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.Supplier)
                .WithMany(s => s.PurchaseOrders)
                .HasForeignKey(po => po.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.Employee)
                .WithMany()
                .HasForeignKey(po => po.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(poi => poi.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(poi => poi.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseOrderItem>()
                .HasOne(poi => poi.PurchaseOrder)
                .WithMany(po => po.Items)
                .HasForeignKey(poi => poi.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseOrderItem>()
                .HasOne(poi => poi.Product)
                .WithMany(p => p.PurchaseOrderItems)
                .HasForeignKey(poi => poi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<WholesaleSaleRequest>()
    .Property(w => w.DiscountRate)
    .HasPrecision(5, 2);

            modelBuilder.Entity<WholesaleSaleRequest>()
                .Property(w => w.SubtotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<WholesaleSaleRequest>()
                .Property(w => w.DiscountAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<WholesaleSaleRequest>()
                .Property(w => w.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<WholesaleSaleRequest>()
                .HasOne(w => w.Customer)
                .WithMany()
                .HasForeignKey(w => w.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WholesaleSaleRequest>()
                .HasOne(w => w.Employee)
                .WithMany()
                .HasForeignKey(w => w.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WholesaleSaleRequest>()
                .HasOne(w => w.Sale)
                .WithMany()
                .HasForeignKey(w => w.SaleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WholesaleSaleRequestItem>()
                .Property(i => i.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<WholesaleSaleRequestItem>()
                .Property(i => i.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<WholesaleSaleRequestItem>()
                .HasOne(i => i.WholesaleSaleRequest)
                .WithMany(w => w.Items)
                .HasForeignKey(i => i.WholesaleSaleRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WholesaleSaleRequestItem>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectModule>().Property(p => p.Budget).HasPrecision(18, 2);
            modelBuilder.Entity<ProjectModule>().Property(p => p.Income).HasPrecision(18, 2);
            modelBuilder.Entity<ProjectModule>().Property(p => p.Expense).HasPrecision(18, 2);

            modelBuilder.Entity<ProjectTask>().Property(t => t.Budget).HasPrecision(18, 2);
            modelBuilder.Entity<ProjectTask>().Property(t => t.Cost).HasPrecision(18, 2);

            modelBuilder.Entity<ProjectTeamMember>()
                .Property(m => m.EstimatedWorkHours)
                .HasPrecision(10, 2);
            modelBuilder.Entity<ProjectTeamMember>()
                .Property(m => m.ActualWorkHours)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ProjectTask>()
                .HasOne(t => t.AssignedMember)
                .WithMany(m => m.AssignedTasks)
                .HasForeignKey(t => t.AssignedMemberId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProjectTask>()
                .HasOne(t => t.DependsOnTask)
                .WithMany(t => t.DependentTasks)
                .HasForeignKey(t => t.DependsOnTaskId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
