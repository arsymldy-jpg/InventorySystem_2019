using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Helpers;

namespace Inventory_Api.Middlewares
{
    public class UserExpiryMiddleware
    {
        private readonly RequestDelegate _next;

        public UserExpiryMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            // اگر کاربر احراز هویت شده است
            if (context.User.Identity.IsAuthenticated)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var roleId = context.User.FindFirst("RoleId")?.Value;

                if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(roleId))
                {
                    var userRole = Roles.GetRoleName(int.Parse(roleId));

                    // فقط برای کاربرانی که Admin یا SeniorUser نیستند بررسی کن
                    if (userRole != Roles.Admin && userRole != Roles.SeniorUser)
                    {
                        var user = await dbContext.Users.FindAsync(int.Parse(userId));

                        if (user != null && user.ExpiryDate.HasValue && user.ExpiryDate.Value < DateTime.UtcNow)
                        {
                            // غیرفعال کردن کاربر اگر تاریخ انقضا گذشته است
                            user.IsActive = false;
                            await dbContext.SaveChangesAsync();

                            context.Response.StatusCode = 401;
                            await context.Response.WriteAsync("حساب کاربری شما منقضی شده است");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}   