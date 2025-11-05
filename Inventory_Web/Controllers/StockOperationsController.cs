using Inventory_Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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

        // GET: StockOperations - مشاهده تاریخچه عملیات
        public async Task<IActionResult> Index()
        {
            try
            {
                var operations = await _apiService.GetAsync<List<StockOperationDto>>("api/StockOperations");
                return View(operations ?? new List<StockOperationDto>());
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت عملیات: {ex.Message}");
                TempData["Error"] = "خطا در دریافت تاریخچه عملیات";
                return View(new List<StockOperationDto>());
            }
        }

        // GET: StockOperations/Receive - فرم ورود کالا
        public async Task<IActionResult> Receive()
        {
            await LoadViewDataForForms();
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

                await LoadViewDataForForms();
                TempData["Error"] = "خطا در ثبت ورود کالا";
                return View(model);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در ثبت ورود کالا: {ex.Message}");
                await LoadViewDataForForms();
                TempData["Error"] = "خطا در ثبت ورود کالا";
                return View(model);
            }
        }

        // GET: StockOperations/Issue - فرم خروج کالا
        public async Task<IActionResult> Issue()
        {
            await LoadViewDataForForms();

            // دریافت مراکز هزینه برای خروج کالا
            var costCenters = await _apiService.GetAsync<List<CostCenterDto>>("api/CostCenters");
            ViewBag.CostCenters = costCenters ?? new List<CostCenterDto>();

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

                await LoadViewDataForForms();
                var costCenters = await _apiService.GetAsync<List<CostCenterDto>>("api/CostCenters");
                ViewBag.CostCenters = costCenters ?? new List<CostCenterDto>();

                TempData["Error"] = "خطا در ثبت خروج کالا";
                return View(model);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در ثبت خروج کالا: {ex.Message}");
                await LoadViewDataForForms();
                var costCenters = await _apiService.GetAsync<List<CostCenterDto>>("api/CostCenters");
                ViewBag.CostCenters = costCenters ?? new List<CostCenterDto>();

                TempData["Error"] = "خطا در ثبت خروج کالا";
                return View(model);
            }
        }

        private async Task LoadViewDataForForms()
        {
            // دریافت لیست محصولات، انبارها و برندها برای dropdownها
            var products = await _apiService.GetAsync<List<StockProductInfo>>("api/Products");
            var warehouses = await _apiService.GetAsync<List<StockWarehouseInfo>>("api/Warehouses");
            var brands = await _apiService.GetAsync<List<StockBrandInfo>>("api/Brands");

            ViewBag.Products = products ?? new List<StockProductInfo>();
            ViewBag.Warehouses = warehouses ?? new List<StockWarehouseInfo>();
            ViewBag.Brands = brands ?? new List<StockBrandInfo>();
        }
    }

    // مدل‌های جدید برای Stock Operations
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

    // مدل‌های اطلاعاتی برای dropdownها
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
}