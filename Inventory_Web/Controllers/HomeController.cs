using Inventory_Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory_Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IApiService _apiService;

        public HomeController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardData = new DashboardViewModel
                {
                    UserName = User.Identity?.Name ?? "کاربر",
                    UserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "بدون نقش",
                    CurrentTime = DateTime.Now.ToString("yyyy/MM/dd - HH:mm")
                };

                // دریافت آمار واقعی از API
                var products = await _apiService.GetAsync<List<StockProductInfo>>("api/Products");
                var warehouses = await _apiService.GetAsync<List<StockWarehouseInfo>>("api/Warehouses");
                var recentOperations = await _apiService.GetAsync<List<StockOperationDto>>("api/StockOperations/recent?count=5");

                dashboardData.TotalProducts = products?.Count ?? 0;
                dashboardData.ActiveWarehouses = warehouses?.Count ?? 0;
                dashboardData.TodayOperations = await GetTodayOperationsCount();
                dashboardData.LowStockItems = await GetLowStockItemsCount();
                dashboardData.RecentActivities = recentOperations ?? new List<StockOperationDto>();

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                // در صورت خطا، داده‌های پیش‌فرض نمایش داده شود
                var defaultData = new DashboardViewModel
                {
                    UserName = User.Identity?.Name ?? "کاربر",
                    UserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "بدون نقش",
                    CurrentTime = DateTime.Now.ToString("yyyy/MM/dd - HH:mm"),
                    TotalProducts = 0,
                    ActiveWarehouses = 0,
                    TodayOperations = 0,
                    LowStockItems = 0,
                    RecentActivities = new List<StockOperationDto>()
                };

                return View(defaultData);
            }
        }

        private async Task<int> GetTodayOperationsCount()
        {
            try
            {
                var todayOperations = await _apiService.GetAsync<List<StockOperationDto>>("api/StockOperations/today");
                return todayOperations?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<int> GetLowStockItemsCount()
        {
            try
            {
                var lowStockItems = await _apiService.GetAsync<List<object>>("api/Inventory/low-stock");
                return lowStockItems?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            return RedirectToAction("Login", "Auth");
        }
    }

    public class DashboardViewModel
    {
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public string CurrentTime { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveWarehouses { get; set; }
        public int TodayOperations { get; set; }
        public int LowStockItems { get; set; }
        public List<StockOperationDto> RecentActivities { get; set; }
    }
}