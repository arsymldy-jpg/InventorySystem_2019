using Inventory_Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory_Web.Controllers
{
    [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper,Storekeeper")]
    public class StockOperationsController : Controller
    {
        private readonly IApiService _apiService;

        public StockOperationsController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: StockOperations - مشاهده تاریخچه عملیات (فیلتر شده براساس دسترسی)
        public async Task<IActionResult> Index()
        {
            try
            {
                var operations = await _apiService.GetAsync<List<StockOperationDto>>("api/StockOperations");

                // برای تست: نمایش همه عملیات بدون فیلتر
                return View(operations ?? new List<StockOperationDto>());
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت عملیات: {ex.Message}");
                TempData["Error"] = "خطا در دریافت تاریخچه عملیات";
                return View(new List<StockOperationDto>());
            }
        }





        // GET: StockOperations/Receive - فرم ورود کالا (فقط انبارهای مجاز)
        public async Task<IActionResult> Receive()
        {
            await LoadViewDataForFormsWithAccess();
            return View();
        }

        // POST: StockOperations/Receive - ثبت ورود کالا
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(ReceiveStockViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // بررسی دسترسی برای انبارداران
                    if (User.IsInRole("Storekeeper") && !User.IsInRole("Admin") && !User.IsInRole("SeniorUser") && !User.IsInRole("SeniorStorekeeper"))
                    {
                        var canEdit = await CanUserEditWarehouse(model.WarehouseId);
                        if (!canEdit)
                        {
                            TempData["Error"] = "شما مجوز ورود کالا به این انبار را ندارید";
                            await LoadViewDataForFormsWithAccess();
                            return View(model);
                        }
                    }

                    var receiveDto = new
                    {
                        ProductId = model.ProductId,
                        WarehouseId = model.WarehouseId,
                        BrandId = model.BrandId,
                        Quantity = model.Quantity,
                        Reason = model.Reason
                    };

                    var result = await _apiService.PostAsync<StockOperationDto>("api/StockOperations/receive", receiveDto);
                    if (result != null)
                    {
                        TempData["Success"] = "ورود کالا با موفقیت ثبت شد";
                        return RedirectToAction(nameof(Index));
                    }
                }

                await LoadViewDataForFormsWithAccess();
                TempData["Error"] = "خطا در ثبت ورود کالا";
                return View(model);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در ثبت ورود کالا: {ex.Message}");
                await LoadViewDataForFormsWithAccess();
                TempData["Error"] = "خطا در ثبت ورود کالا";
                return View(model);
            }
        }

        // GET: StockOperations/Issue - فرم خروج کالا
        public async Task<IActionResult> Issue()
        {
            // فقط کالاها و مراکز هزینه را بارگذاری می‌کنیم
            var products = await GetProductsForStorekeeper();
            ViewBag.Products = products;

            var costCentersList = await _apiService.GetAsync<List<CostCenterDto>>("api/CostCenters");
            ViewBag.CostCenters = costCentersList ?? new List<CostCenterDto>();

            // انبارها به صورت پویا بارگذاری می‌شوند
            ViewBag.Warehouses = new List<StockWarehouseInfo>();

            return View();
        }


        // POST: StockOperations/Issue - ثبت خروج کالا
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Issue(IssueStockViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // بررسی دسترسی برای انبارداران
                    if (User.IsInRole("Storekeeper") && !User.IsInRole("Admin") && !User.IsInRole("SeniorUser") && !User.IsInRole("SeniorStorekeeper"))
                    {
                        var canEdit = await CanUserEditWarehouse(model.WarehouseId);
                        if (!canEdit)
                        {
                            TempData["Error"] = "شما مجوز خروج کالا از این انبار را ندارید";
                            await LoadViewDataForFormsWithAccess();
                            var costCentersList = await _apiService.GetAsync<List<CostCenterDto>>("api/CostCenters");
                            ViewBag.CostCenters = costCentersList ?? new List<CostCenterDto>();
                            return View(model);
                        }
                    }

                    var issueDto = new
                    {
                        ProductId = model.ProductId,
                        WarehouseId = model.WarehouseId,
                        BrandId = model.BrandId,
                        Quantity = model.Quantity,
                        CostCenterId = model.CostCenterId,
                        Reason = model.Reason
                    };

                    var result = await _apiService.PostAsync<StockOperationDto>("api/StockOperations/issue", issueDto);
                    if (result != null)
                    {
                        TempData["Success"] = "خروج کالا با موفقیت ثبت شد";
                        return RedirectToAction(nameof(Index));
                    }
                }

                await LoadViewDataForFormsWithAccess();
                var costCentersList2 = await _apiService.GetAsync<List<CostCenterDto>>("api/CostCenters");
                ViewBag.CostCenters = costCentersList2 ?? new List<CostCenterDto>();

                TempData["Error"] = "خطا در ثبت خروج کالا";
                return View(model);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در ثبت خروج کالا: {ex.Message}");
                await LoadViewDataForFormsWithAccess();
                var costCentersList3 = await _apiService.GetAsync<List<CostCenterDto>>("api/CostCenters");
                ViewBag.CostCenters = costCentersList3 ?? new List<CostCenterDto>();

                TempData["Error"] = "خطا در ثبت خروج کالا";
                return View(model);
            }
        }

        // متد کمکی برای بارگذاری داده‌ها با درنظرگیری دسترسی‌ها
        // متد کمکی برای بارگذاری داده‌ها با درنظرگیری دسترسی‌ها
        private async Task LoadViewDataForFormsWithAccess()
        {
            // برای دیباگ
            await TestEndpoints();

            // 1. محصولات - تمام محصولات
            var products = await GetProductsForStorekeeper();
            ViewBag.Products = products;

            // 2. انبارها - فقط انبارهای قابل ویرایش
            var warehouses = await GetWarehousesForStorekeeper();
            ViewBag.Warehouses = warehouses;

            // 3. برندها - تمام برندها
            var brands = await GetBrandsForStorekeeper();
            ViewBag.Brands = brands;

            Console.WriteLine($"📊 نتیجه نهایی - محصولات: {products.Count}, انبارها: {warehouses.Count}, برندها: {brands.Count}");
        }


        // دریافت تمام محصولات
        private async Task<List<StockProductInfo>> GetProductsForStorekeeper()
        {
            try
            {
                var products = await _apiService.GetAsync<List<StockProductInfo>>("api/Products");
                return products ?? new List<StockProductInfo>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطا در دریافت محصولات: {ex.Message}");
                return new List<StockProductInfo>();
            }
        }

        // دریافت انبارهای قابل ویرایش برای انباردار
        // دریافت انبارها - راه حل ساده
        // دریافت انبارهای قابل ویرایش برای انباردار
        // دریافت انبارهای قابل ویرایش برای انباردار
        private async Task<List<StockWarehouseInfo>> GetWarehousesForStorekeeper()
        {
            try
            {
                if (User.IsInRole("Storekeeper"))// && !User.IsInRole("Admin") && !User.IsInRole("SeniorUser") && !User.IsInRole("SeniorStorekeeper"))
                {
                    // استفاده از endpoint ای که کار می‌کند
                    var userId = GetCurrentUserId();
                    var userAccess = await _apiService.GetAsync<List<WarehouseAccessInfo>>($"api/WarehouseAccess/user/{userId}");

                    if (userAccess != null && userAccess.Any())
                    {
                        // فقط انبارهای قابل ویرایش
                        var editableWarehouseIds = userAccess
                            .Where(a => a.CanEdit)
                            .Select(a => a.WarehouseId)
                            .ToList();

                        // دریافت تمام انبارها
                        var allWarehouses = await _apiService.GetAsync<List<StockWarehouseInfo>>("api/Warehouses");

                        if (allWarehouses != null)
                        {
                            // فیلتر انبارهای قابل ویرایش
                            var editableWarehouses = allWarehouses
                                .Where(w => editableWarehouseIds.Contains(w.Id))
                                .ToList();

                            Console.WriteLine($"✅ انبارهای قابل ویرایش: {editableWarehouses.Count} از {allWarehouses.Count}");
                            return editableWarehouses;
                        }
                    }
                    else
                    {
                        Console.WriteLine("⚠️ کاربر انباردار دسترسی ویرایش به هیچ انباری ندارد");
                    }

                    return new List<StockWarehouseInfo>();
                }
                else
                {
                    // سایر نقش‌ها تمام انبارها را می‌بینند
                    var allWarehouses = await _apiService.GetAsync<List<StockWarehouseInfo>>("api/Warehouses");
                    return allWarehouses ?? new List<StockWarehouseInfo>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطا در دریافت انبارها: {ex.Message}");
                return new List<StockWarehouseInfo>();
            }
        }


        // دریافت تمام برندها
        // دریافت برندها - راه حل ساده و مطمئن
        private async Task<List<BrandInfo>> GetBrandsForStorekeeper()
        {
            try
            {
                // راه حل: استفاده مستقیم از endpoint محصولات که کار می‌کند
                var brands = await _apiService.GetAsync<List<BrandInfo>>("api/brands");

                if (brands != null)
                    return brands;
                else
                    return new List<BrandInfo>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطا در دریافت برندها: {ex.Message}");

                // داده‌های پیشفرض برای جلوگیری از خطا
                return new List<BrandInfo>();
            }
        }


        // متد کمکی برای گرفتن شناسه کاربر جاری
        private int GetCurrentUserId()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                return int.Parse(userIdClaim ?? "0");
            }
            catch
            {
                return 0;
            }
        }

        // متدهای کمکی برای مدیریت دسترسی
        private async Task<List<int>> GetAccessibleWarehousesForUser()
        {
            try
            {
                var accessList = await _apiService.GetAsync<List<WarehouseAccessInfoModel>>($"api/WarehouseAccess/my-access");
                // انبارهایی که کاربر دسترسی مشاهده یا ویرایش دارد
                return accessList?.Where(a => a.CanView || a.CanEdit).Select(a => a.WarehouseId).ToList() ?? new List<int>();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت دسترسی‌ها: {ex.Message}");
                return new List<int>();
            }
        }

        private async Task<List<StockWarehouseInfo>> GetEditableWarehousesForUser()
        {
            try
            {
                // استفاده از endpoint صحیح
                var accessList = await _apiService.GetAsync<List<WarehouseAccessInfoModel>>($"api/WarehouseAccess/my-access");
                var editableWarehouseIds = accessList?.Where(a => a.CanEdit).Select(a => a.WarehouseId).ToList() ?? new List<int>();

                if (editableWarehouseIds.Any())
                {
                    var allWarehouses = await _apiService.GetAsync<List<StockWarehouseInfo>>("api/Warehouses");
                    return allWarehouses?.Where(w => editableWarehouseIds.Contains(w.Id)).ToList() ?? new List<StockWarehouseInfo>();
                }

                return new List<StockWarehouseInfo>();
            }
            catch
            {
                return new List<StockWarehouseInfo>();
            }
        }


        private async Task<bool> CanUserEditWarehouse(int warehouseId)
        {
            try
            {
                // استفاده از endpoint صحیح
                var accessList = await _apiService.GetAsync<List<WarehouseAccessInfoModel>>($"api/WarehouseAccess/my-access");
                return accessList?.Any(a => a.WarehouseId == warehouseId && a.CanEdit) ?? false;
            }
            catch
            {
                return false;
            }
        }

        // متد برای تست و دیباگ
        private async Task TestEndpoints()
        {
            try
            {
                Console.WriteLine("🔍 تست endpointهای مختلف...");

                // تست endpoint انبارها
                var warehouses = await _apiService.GetAsync<List<StockWarehouseInfo>>("api/Warehouses");
                Console.WriteLine($"📦 انبارها: {warehouses?.Count ?? 0}");

                // تست endpoint دسترسی‌ها
                var userId = GetCurrentUserId();
                var userAccess = await _apiService.GetAsync<List<WarehouseAccessSimpleDto>>($"api/WarehouseAccess/user/{userId}");
                Console.WriteLine($"🔑 دسترسی‌های کاربر: {userAccess?.Count ?? 0}");

                // تست endpoint برندها
                var brands = await _apiService.GetAsync<List<BrandSimpleDto>>("api/Brands");
                Console.WriteLine($"🏷️ برندها: {brands?.Count ?? 0}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطا در تست: {ex.Message}");
            }
        }
    }



    public class WarehouseAccessSimpleDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public bool CanEdit { get; set; }
        public bool CanView { get; set; }
    }

    public class BrandSimpleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // مدل‌های ساده برای داده‌های پایه
    public class ProductSimpleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MainCode { get; set; }
    }

    public class WarehouseSimpleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // مدل جدید برای اطلاعات دسترسی انبار (با نام متفاوت)
    public class WarehouseAccessInfoModel
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
    }

    // مدل‌های اصلی (همان‌هایی که از قبل وجود داشتند)
    public class StockOperationDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int Quantity { get; set; }
        public string OperationType { get; set; }
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public string Reason { get; set; }
        public System.DateTime OperationDate { get; set; }
        public string CreatedByName { get; set; }
    }

    public class ReceiveStockViewModel
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int BrandId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; }
    }

    public class IssueStockViewModel
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int BrandId { get; set; }
        public int Quantity { get; set; }
        public int CostCenterId { get; set; }
        public string Reason { get; set; }
    }

    public class CostCenterDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class StockProductInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MainCode { get; set; }
    }

    public class StockWarehouseInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class StockBrandInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class WarehouseAccessInfo
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public bool CanEdit { get; set; }
        public bool CanView { get; set; }
    }


}