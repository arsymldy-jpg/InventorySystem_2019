using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory_Web.Services;

namespace Inventory_Web.Controllers
{
    [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper,Storekeeper,Viewer")]
    public class BrandsController : Controller
    {
        private readonly IApiService _apiService;

        public BrandsController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Brands - همه نقش‌ها می‌توانند مشاهده کنند
        public async Task<IActionResult> Index()
        {
            try
            {
                var brands = await _apiService.GetAsync<List<BrandDto>>("api/Brands");
                return View(brands ?? new List<BrandDto>());
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت برندها: {ex.Message}");
                TempData["Error"] = "خطا در دریافت لیست برندها";
                return View(new List<BrandDto>());
            }
        }

        // GET: Brands/Details/5 - همه نقش‌ها می‌توانند مشاهده کنند
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var brand = await _apiService.GetAsync<BrandDto>($"api/Brands/{id}");
                if (brand == null)
                {
                    return NotFound();
                }
                return View(brand);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت جزئیات برند: {ex.Message}");
                TempData["Error"] = "خطا در دریافت اطلاعات برند";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Brands/Create - فقط Admin, SeniorUser, SeniorStorekeeper
        [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Brands/Create - فقط Admin, SeniorUser, SeniorStorekeeper
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper")]
        public async Task<IActionResult> Create(CreateBrandDto createBrandDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _apiService.PostAsync<BrandDto>("api/Brands", createBrandDto);
                    if (result != null)
                    {
                        TempData["Success"] = "برند با موفقیت ایجاد شد";
                        return RedirectToAction(nameof(Index));
                    }
                }
                TempData["Error"] = "خطا در ایجاد برند";
                return View(createBrandDto);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در ایجاد برند: {ex.Message}");
                TempData["Error"] = "خطا در ایجاد برند";
                return View(createBrandDto);
            }
        }

        // GET: Brands/Edit/5 - فقط Admin, SeniorUser, SeniorStorekeeper
        [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                System.Console.WriteLine($"🔍 درخواست ویرایش برند با شناسه: {id}");

                var brand = await _apiService.GetAsync<BrandDto>($"api/Brands/{id}");

                System.Console.WriteLine($"📦 پاسخ دریافت شده: {brand != null}");

                if (brand == null)
                {
                    System.Console.WriteLine("❌ برند یافت نشد");
                    return NotFound();
                }

                System.Console.WriteLine($"✅ برند یافت شد: {brand.Name}");
                return View(brand);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت اطلاعات برای ویرایش: {ex.Message}");
                System.Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                TempData["Error"] = "خطا در دریافت اطلاعات برند";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Brands/Edit/5 - فقط Admin, SeniorUser, SeniorStorekeeper
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper")]
        public async Task<IActionResult> Edit(int id, BrandDto brandDto)
        {
            try
            {
                if (id != brandDto.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    var success = await _apiService.PutAsync<bool>($"api/Brands/{id}", brandDto);
                    if (success)
                    {
                        TempData["Success"] = "برند با موفقیت ویرایش شد";
                        return RedirectToAction(nameof(Index));
                    }
                }
                TempData["Error"] = "خطا در ویرایش برند";
                return View(brandDto);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در ویرایش برند: {ex.Message}");
                TempData["Error"] = "خطا در ویرایش برند";
                return View(brandDto);
            }
        }
    }

    public class BrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateBrandDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}