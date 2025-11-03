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
            if (context.Users.Any())
            {
                return; // دیتابیس قبلاً سید شده
            }

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
            context.SaveChanges();
        }
    }
}