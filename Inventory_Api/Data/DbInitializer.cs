using System;
using System.Linq;
using Inventory_Api.Models.Entities;
using Inventory_Api.Services;

namespace Inventory_Api.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context, PasswordHasherService passwordHasher)
        {
            context.Database.EnsureCreated();

            // بررسی وجود کاربران
            if (!context.Users.Any())
            {
                // ایجاد کاربر ادمین با رمز هش شده
                var adminUser = new User
                {
                    FirstName = "مدیر",
                    LastName = "سیستم",
                    PersonnelCode = "000000",
                    Mobile = "09123456789",
                    Email = "admin@inventory.com",
                    PasswordHash = passwordHasher.HashPassword("Admin123!"),
                    RoleId = 1, // Admin
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
            }

            

            if(!context.Products.Any())
            {
                var product1 = new Product
                {
                    Name = "A",
                    MainCode = "1001",
                };
                context.Products.Add(product1);

                var product2 = new Product
                {
                    Name = "B",
                    MainCode = "1002",
                };
                context.Products.Add(product2);
            }

            if(!context.Brands.Any())
            {
                var brand1 = new Brand
                {
                    Name = "brand A",
                };
                context.Brands.Add(brand1);

                var brand2 = new Brand
                {
                    Name = "brand B",
                };
                context.Brands.Add(brand2);
            }

            if(!context.Warehouses.Any())
            {
                var warehouse1 = new Warehouse
                {
                    Name = "store A",
                };
                context.Warehouses.Add(warehouse1);

                var warehouse2 = new Warehouse
                {
                    Name = "store B",
                };
                context.Warehouses.Add(warehouse2);
            }


            context.SaveChanges();
        }
    }
}