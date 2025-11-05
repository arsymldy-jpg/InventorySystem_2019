// Middlewares/DebugMiddleware.cs
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Inventory_Web.Middlewares
{
    public class DebugMiddleware
    {
        private readonly RequestDelegate _next;

        public DebugMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // لاگ کردن اطلاعات احراز هویت برای هر درخواست
            var isAuthenticated = context.User.Identity.IsAuthenticated;
            var userName = context.User.Identity.Name;
            var authCookie = context.Request.Cookies["InventoryAuth"];

            System.Console.WriteLine($"=== Debug Middleware ===");
            System.Console.WriteLine($"Path: {context.Request.Path}");
            System.Console.WriteLine($"Authenticated: {isAuthenticated}");
            System.Console.WriteLine($"User: {userName}");
            System.Console.WriteLine($"Auth Cookie Exists: {!string.IsNullOrEmpty(authCookie)}");
            System.Console.WriteLine($"=== End Debug ===");

            await _next(context);
        }
    }
}