using Microsoft.EntityFrameworkCore;
using Inventory_Api.Models.Entities;
using System;

namespace Inventory_Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductBrand> ProductBrands { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<CostCenter> CostCenters { get; set; }
        public DbSet<WarehouseAccess> WarehouseAccesses { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<StockOperation> StockOperations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User-Role Relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            // Product-Brand Many-to-Many
            modelBuilder.Entity<ProductBrand>()
                .HasKey(pb => pb.Id);

            modelBuilder.Entity<ProductBrand>()
                .HasOne(pb => pb.Product)
                .WithMany(p => p.ProductBrands)
                .HasForeignKey(pb => pb.ProductId);

            modelBuilder.Entity<ProductBrand>()
                .HasOne(pb => pb.Brand)
                .WithMany(b => b.ProductBrands)
                .HasForeignKey(pb => pb.BrandId);

            // Inventory Relationships
            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Product)
                .WithMany(p => p.Inventories)
                .HasForeignKey(i => i.ProductId);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Warehouse)
                .WithMany(w => w.Inventories)
                .HasForeignKey(i => i.WarehouseId);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Brand)
                .WithMany()
                .HasForeignKey(i => i.BrandId);

            // Warehouse Access
            modelBuilder.Entity<WarehouseAccess>()
                .HasOne(wa => wa.User)
                .WithMany(u => u.WarehouseAccesses)
                .HasForeignKey(wa => wa.UserId);

            modelBuilder.Entity<WarehouseAccess>()
                .HasOne(wa => wa.Warehouse)
                .WithMany(w => w.WarehouseAccesses)
                .HasForeignKey(wa => wa.WarehouseId);

            // Audit Log
            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserId);

            #region StockOperation Relationships
            modelBuilder.Entity<StockOperation>()
                .HasOne(so => so.Product)
                .WithMany()
                .HasForeignKey(so => so.ProductId);

            modelBuilder.Entity<StockOperation>()
                .HasOne(so => so.Warehouse)
                .WithMany()
                .HasForeignKey(so => so.WarehouseId);

            modelBuilder.Entity<StockOperation>()
                .HasOne(so => so.Brand)
                .WithMany()
                .HasForeignKey(so => so.BrandId);

            modelBuilder.Entity<StockOperation>()
                .HasOne(so => so.CostCenter)
                .WithMany()
                .HasForeignKey(so => so.CostCenterId);

            modelBuilder.Entity<StockOperation>()
                .HasOne(so => so.CreatedByUser)
                .WithMany()
                .HasForeignKey(so => so.CreatedBy);
            #endregion


            // Seed Initial Data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", Description = "مدیر سیستم - دسترسی کامل" },
                new Role { Id = 2, Name = "SeniorUser", Description = "کاربر ارشد - دسترسی گسترده به جز مدیریت کاربران ارشد و ادمین" },
                new Role { Id = 3, Name = "SeniorStorekeeper", Description = "انباردار ارشد - مدیریت انبارداران و موجودی ها" },
                new Role { Id = 4, Name = "Storekeeper", Description = "انباردار - مدیریت موجودی انبارهای زیر مجموعه" },
                new Role { Id = 5, Name = "Viewer", Description = "ناظر - فقط مشاهده اطلاعات" }
            );

            // Seed Default Warehouse
            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse
                {
                    Id = 1,
                    Name = "انبار اصلی",
                    Address = "آدرس انبار اصلی",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            );
        }
    }
}