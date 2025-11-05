using Inventory_Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory_Web.Controllers
{
    [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper,Storekeeper")]
    public class InventoryController : Controller
    {
        private readonly IApiService _apiService;

        public InventoryController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Inventory - مشاهده موجودی همه انبارها
        public async Task<IActionResult> Index()
        {
            try
            {
                var inventory = await _apiService.GetAsync<List<InventoryItemDto>>("api/Inventory");
                return View(inventory ?? new List<InventoryItemDto>());
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت موجودی: {ex.Message}");
                TempData["Error"] = "خطا در دریافت لیست موجودی";
                return View(new List<InventoryItemDto>());
            }
        }

        // GET: Inventory/Warehouse/5 - موجودی یک انبار خاص
        public async Task<IActionResult> Warehouse(int id)
        {
            try
            {
                var inventory = await _apiService.GetAsync<List<InventoryItemDto>>($"api/Inventory/warehouse/{id}");
                ViewBag.WarehouseId = id;
                return View("Index", inventory ?? new List<InventoryItemDto>());
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت موجودی انبار: {ex.Message}");
                TempData["Error"] = "خطا در دریافت موجودی انبار";
                return View("Index", new List<InventoryItemDto>());
            }
        }

        // GET: Inventory/Adjust/5 - فرم تنظیم موجودی
        [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper,Storekeeper")]
        public async Task<IActionResult> Adjust(int productId, int warehouseId, int brandId)
        {
            try
            {
                // دریافت اطلاعات محصول، انبار و برند
                var product = await _apiService.GetAsync<InventoryProductInfo>($"api/Products/{productId}");
                var warehouse = await _apiService.GetAsync<InventoryWarehouseInfo>($"api/Warehouses/{warehouseId}");
                var brand = await _apiService.GetAsync<InventoryBrandInfo>($"api/Brands/{brandId}");

                if (product == null || warehouse == null || brand == null)
                {
                    TempData["Error"] = "اطلاعات یافت نشد";
                    return RedirectToAction(nameof(Index));
                }

                // دریافت موجودی فعلی
                var currentInventory = await _apiService.GetAsync<List<InventoryItemDto>>("api/Inventory");
                var currentItem = currentInventory?.FirstOrDefault(i =>
                    i.ProductId == productId && i.WarehouseId == warehouseId && i.BrandId == brandId);

                var viewModel = new AdjustInventoryViewModel
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    WarehouseId = warehouseId,
                    WarehouseName = warehouse.Name,
                    BrandId = brandId,
                    BrandName = brand.Name,
                    CurrentQuantity = currentItem?.Quantity ?? 0
                };

                return View(viewModel);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت اطلاعات برای تنظیم موجودی: {ex.Message}");
                TempData["Error"] = "خطا در دریافت اطلاعات";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Inventory/Adjust - تنظیم موجودی
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper,Storekeeper")]
        public async Task<IActionResult> Adjust(AdjustInventoryViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var adjustDto = new
                    {
                        ProductId = model.ProductId,
                        WarehouseId = model.WarehouseId,
                        BrandId = model.BrandId,
                        NewQuantity = model.NewQuantity,
                        Reason = model.Reason
                    };

                    var result = await _apiService.PostAsync<InventoryItemDto>("api/Inventory/adjust", adjustDto);
                    if (result != null)
                    {
                        TempData["Success"] = "موجودی با موفقیت تنظیم شد";
                        return RedirectToAction(nameof(Index));
                    }
                }
                TempData["Error"] = "خطا در تنظیم موجودی";
                return View(model);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در تنظیم موجودی: {ex.Message}");
                TempData["Error"] = "خطا در تنظیم موجودی";
                return View(model);
            }
        }
    }

    // مدل‌های جدید با نام‌های منحصربفرد
    public class InventoryItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int Quantity { get; set; }
        public System.DateTime LastUpdated { get; set; }
    }

    public class AdjustInventoryViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int CurrentQuantity { get; set; }
        public int NewQuantity { get; set; }
        public string Reason { get; set; }
    }

    public class InventoryProductInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class InventoryWarehouseInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class InventoryBrandInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}