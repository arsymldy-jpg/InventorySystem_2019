using Inventory_Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory_Web.Controllers
{
    [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper,Storekeeper,Viewer")]
    public class ProductBrandsController : Controller
    {
        private readonly IApiService _apiService;

        public ProductBrandsController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: ProductBrands/Manage/5 - مدیریت برندهای یک کالا
        [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper")]
        public async Task<IActionResult> Manage(int productId)
        {
            try
            {
                System.Console.WriteLine($"🔍 شروع مدیریت برندها برای کالا: {productId}");

                // دریافت اطلاعات کالا با مدل مشخص
                var product = await _apiService.GetAsync<ProductInfo>($"api/Products/{productId}");
                if (product == null)
                {
                    TempData["Error"] = "کالا یافت نشد";
                    return RedirectToAction("Index", "Products");
                }

                // دریافت برندهای کالا با مدل مشخص
                List<ProductBrandInfo> productBrands = new List<ProductBrandInfo>();
                try
                {
                    var brandsResult = await _apiService.GetAsync<List<ProductBrandInfo>>($"api/ProductBrands/product/{productId}");
                    productBrands = brandsResult ?? new List<ProductBrandInfo>();
                }
                catch
                {
                    System.Console.WriteLine("⚠️ خطا در دریافت برندهای کالا - ادامه با لیست خالی");
                }

                // دریافت لیست تمام برندها با مدل مشخص
                List<BrandInfo> allBrands = new List<BrandInfo>();
                try
                {
                    var allBrandsResult = await _apiService.GetAsync<List<BrandInfo>>("api/Brands");
                    allBrands = allBrandsResult ?? new List<BrandInfo>();
                }
                catch
                {
                    System.Console.WriteLine("⚠️ خطا در دریافت لیست برندها - ادامه با لیست خالی");
                }

                var viewModel = new ProductBrandsManageViewModel
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    ProductMainCode = product.MainCode,
                    CurrentBrands = productBrands,
                    AvailableBrands = allBrands
                };

                System.Console.WriteLine("✅ مدیریت برندها با موفقیت لود شد");
                return View(viewModel);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطای کلی در مدیریت برندها: {ex.Message}");
                TempData["Error"] = "خطا در بارگذاری صفحه مدیریت برندها";
                return RedirectToAction("Index", "Products");
            }
        }

        // به فایل ProductBrandsController.cs در پروژه Web اضافه شود

        // GET: api/ProductBrands/product/{productId} - دریافت برندهای یک کالا (برای استفاده در JavaScript)
        [HttpGet("api/ProductBrands/product/{productId}")]
        [AllowAnonymous] // یا [Authorize] اگر نیاز به احراز هویت دارد
        public async Task<IActionResult> GetProductBrandsApi(int productId)
        {
            try
            {
                var productBrands = await _apiService.GetAsync<List<ProductBrandInfo>>($"api/ProductBrands/product/{productId}");
                return Ok(productBrands ?? new List<ProductBrandInfo>());
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت برندهای کالا از API: {ex.Message}");
                return StatusCode(500, new { error = "خطا در دریافت برندهای کالا" });
            }
        }



        // POST: ProductBrands/AddBrand - افزودن برند به کالا
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper")]
        public async Task<IActionResult> AddBrand(int productId, int brandId)
        {
            try
            {
                var createDto = new { ProductId = productId, BrandId = brandId };
                var result = await _apiService.PostAsync<object>("api/ProductBrands", createDto);

                if (result != null)
                {
                    TempData["Success"] = "برند با موفقیت به کالا اضافه شد";
                }
                else
                {
                    TempData["Error"] = "خطا در افزودن برند به کالا";
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در افزودن برند به کالا: {ex.Message}");

                if (ex.Message.Contains("این برند قبلاً به کالا اضافه شده است"))
                {
                    TempData["Error"] = "این برند قبلاً به کالا اضافه شده است";
                }
                else
                {
                    TempData["Error"] = "خطا در افزودن برند به کالا";
                }
            }

            return RedirectToAction("Manage", new { productId = productId });
        }

        // POST: ProductBrands/Remove/5 - حذف برند از کالا
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper")]
        public async Task<IActionResult> Remove(int id, int productId)
        {
            try
            {
                var success = await _apiService.DeleteAsync($"api/ProductBrands/{id}");
                if (success)
                {
                    TempData["Success"] = "برند با موفقیت از کالا حذف شد";
                }
                else
                {
                    TempData["Error"] = "خطا در حذف برند از کالا";
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در حذف برند از کالا: {ex.Message}");
                TempData["Error"] = "خطا در حذف برند از کالا";
            }

            return RedirectToAction("Manage", new { productId = productId });
        }
    }

    // اگر در فایل ProductBrandsController وب نیاز است
    public class ProductBrandApiResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
    }

    // مدل‌های کمکی
    public class ProductBrandsManageViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductMainCode { get; set; }
        public List<ProductBrandInfo> CurrentBrands { get; set; }
        public List<BrandInfo> AvailableBrands { get; set; }
    }

    public class ProductInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MainCode { get; set; }
    }

    public class ProductBrandInfo
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
    }

    public class BrandInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}