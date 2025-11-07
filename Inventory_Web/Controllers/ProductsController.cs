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
    public class ProductsController : Controller
    {
        private readonly IApiService _apiService;

        public ProductsController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Products - همه نقش‌ها می‌توانند مشاهده کنند
        public async Task<IActionResult> Index(string searchString)
        {
            try
            {
                var products = await _apiService.GetAsync<List<ProductDto>>("api/Products");

                if (products == null)
                {
                    products = new List<ProductDto>();
                }

                // اعمال فیلتر جستجو اگر مقدار داشته باشد
                if (!string.IsNullOrEmpty(searchString))
                {
                    products = products.Where(p =>
                        (p.Name != null && p.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                        (p.MainCode != null && p.MainCode.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                        (p.Name2 != null && p.Name2.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                        (p.Code2 != null && p.Code2.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                        (p.Code3 != null && p.Code3.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    ).ToList();

                    ViewBag.SearchString = searchString;
                }

                return View(products);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت کالاها: {ex.Message}");
                TempData["Error"] = "خطا در دریافت لیست کالاها";
                return View(new List<ProductDto>());
            }
        }


        // GET: Products/Details/5 - همه نقش‌ها می‌توانند مشاهده کنند
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _apiService.GetAsync<ProductDto>($"api/Products/{id}");
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت جزئیات: {ex.Message}");
                TempData["Error"] = "خطا در دریافت اطلاعات کالا";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Products/Create - فقط Admin, SeniorUser, SeniorStorekeeper
        [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create - فقط Admin, SeniorUser, SeniorStorekeeper
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper")]
        public async Task<IActionResult> Create(CreateProductDto createProductDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _apiService.PostAsync<ProductDto>("api/Products", createProductDto);
                    if (result != null)
                    {
                        TempData["Success"] = "کالا با موفقیت ایجاد شد";
                        return RedirectToAction(nameof(Index));
                    }
                }
                TempData["Error"] = "خطا در ایجاد کالا";
                return View(createProductDto);
            }
            catch (System.Exception ex)
            {
                // 🔥 مدیریت خطاهای خاص از API
                if (ex.Message.Contains("VALIDATION_ERROR:"))
                {
                    var errorJson = ex.Message.Replace("VALIDATION_ERROR:", "");
                    try
                    {
                        var errorObj = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(errorJson);
                        if (errorObj?.Message != null)
                        {
                            TempData["Error"] = errorObj.Message;
                        }
                        else if (errorJson.Contains("کد اصلی کالا تکراری است"))
                        {
                            TempData["Error"] = "کد اصلی کالا تکراری است. لطفاً کد دیگری انتخاب کنید.";
                        }
                        else if (errorJson.Contains("تکراری است"))
                        {
                            TempData["Error"] = "این مقدار تکراری است. لطفاً مقدار دیگری وارد کنید.";
                        }
                        else
                        {
                            TempData["Error"] = "خطا در ایجاد کالا: " + errorJson;
                        }
                    }
                    catch
                    {
                        TempData["Error"] = "خطا در ایجاد کالا";
                    }
                }
                else
                {
                    System.Console.WriteLine($"❌ خطا در ایجاد کالا: {ex.Message}");
                    TempData["Error"] = "خطا در ایجاد کالا";
                }
                return View(createProductDto);
            }
        }

        // GET: Products/Edit/5 - فقط Admin, SeniorUser, SeniorStorekeeper
        [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var product = await _apiService.GetAsync<ProductDto>($"api/Products/{id}");
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت اطلاعات برای ویرایش: {ex.Message}");
                TempData["Error"] = "خطا در دریافت اطلاعات کالا";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Products/Edit/5 - فقط Admin, SeniorUser, SeniorStorekeeper
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper")]
        public async Task<IActionResult> Edit(int id, ProductDto productDto)
        {
            try
            {
                if (id != productDto.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    var success = await _apiService.PutAsync<bool>($"api/Products/{id}", productDto);
                    if (success)
                    {
                        TempData["Success"] = "کالا با موفقیت ویرایش شد";
                        return RedirectToAction(nameof(Index));
                    }
                }
                TempData["Error"] = "خطا در ویرایش کالا";
                return View(productDto);
            }
            catch (System.Exception ex)
            {
                // 🔥 مدیریت خطاهای خاص از API
                if (ex.Message.Contains("VALIDATION_ERROR:"))
                {
                    var errorJson = ex.Message.Replace("VALIDATION_ERROR:", "");
                    try
                    {
                        var errorObj = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(errorJson);
                        if (errorObj?.Message != null)
                        {
                            TempData["Error"] = errorObj.Message;
                        }
                        else if (errorJson.Contains("کد اصلی کالا تکراری است"))
                        {
                            TempData["Error"] = "کد اصلی کالا تکراری است. لطفاً کد دیگری انتخاب کنید.";
                        }
                        else if (errorJson.Contains("تکراری است"))
                        {
                            TempData["Error"] = "این مقدار تکراری است. لطفاً مقدار دیگری وارد کنید.";
                        }
                        else
                        {
                            TempData["Error"] = "خطا در ویرایش کالا: " + errorJson;
                        }
                    }
                    catch
                    {
                        TempData["Error"] = "خطا در ویرایش کالا";
                    }
                }
                else
                {
                    System.Console.WriteLine($"❌ خطا در ویرایش کالا: {ex.Message}");
                    TempData["Error"] = "خطا در ویرایش کالا";
                }
                return View(productDto);
            }
        }

        // POST: Products/Delete/5 - فقط Admin, SeniorUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SeniorUser")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _apiService.DeleteAsync($"api/Products/{id}");
                if (success)
                {
                    TempData["Success"] = "کالا با موفقیت حذف شد";
                }
                else
                {
                    TempData["Error"] = "خطا در حذف کالا";
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در حذف کالا: {ex.Message}");
                TempData["Error"] = "خطا در حذف کالا";
            }

            return RedirectToAction(nameof(Index));
        }
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string MainCode { get; set; }
        public string Code2 { get; set; }
        public string Code3 { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal ReorderPoint { get; set; }
        public decimal SafetyStock { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateProductDto
    {
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string MainCode { get; set; }
        public string Code2 { get; set; }
        public string Code3 { get; set; }
        public decimal ReorderPoint { get; set; }
        public decimal SafetyStock { get; set; }
    }

    // 🔥 کلاس کمکی برای خطاها
    public class ErrorResponse
    {
        public string Message { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string TraceId { get; set; }
    }


}