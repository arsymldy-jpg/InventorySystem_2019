using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory_Web.Services;

namespace Inventory_Web.Controllers
{
    [Authorize]
    public class WarehousesController : Controller
    {
        private readonly IApiService _apiService;

        public WarehousesController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Warehouses - همه نقش‌ها می‌توانند مشاهده کنند
        public async Task<IActionResult> Index()
        {
            try
            {
                var warehouses = await _apiService.GetAsync<List<WarehouseDto>>("api/Warehouses");
                return View(warehouses ?? new List<WarehouseDto>());
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت انبارها: {ex.Message}");
                TempData["Error"] = "خطا در دریافت لیست انبارها";
                return View(new List<WarehouseDto>());
            }
        }

        // GET: Warehouses/Details/5 - همه نقش‌ها می‌توانند مشاهده کنند
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var warehouse = await _apiService.GetAsync<WarehouseDto>($"api/Warehouses/{id}");
                if (warehouse == null)
                {
                    return NotFound();
                }
                return View(warehouse);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت جزئیات انبار: {ex.Message}");
                TempData["Error"] = "خطا در دریافت اطلاعات انبار";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Warehouses/Create - فقط Admin, SeniorUser
        [Authorize(Roles = "Admin,SeniorUser")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Warehouses/Create - فقط Admin, SeniorUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SeniorUser")]
        public async Task<IActionResult> Create(CreateWarehouseDto createWarehouseDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _apiService.PostAsync<WarehouseDto>("api/Warehouses", createWarehouseDto);
                    if (result != null)
                    {
                        TempData["Success"] = "انبار با موفقیت ایجاد شد";
                        return RedirectToAction(nameof(Index));
                    }
                }
                TempData["Error"] = "خطا در ایجاد انبار";
                return View(createWarehouseDto);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در ایجاد انبار: {ex.Message}");
                TempData["Error"] = "خطا در ایجاد انبار";
                return View(createWarehouseDto);
            }
        }

        // GET: Warehouses/Edit/5 - فقط Admin, SeniorUser
        [Authorize(Roles = "Admin,SeniorUser")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var warehouse = await _apiService.GetAsync<WarehouseDto>($"api/Warehouses/{id}");
                if (warehouse == null)
                {
                    return NotFound();
                }
                return View(warehouse);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت اطلاعات برای ویرایش: {ex.Message}");
                TempData["Error"] = "خطا در دریافت اطلاعات انبار";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Warehouses/Edit/5 - فقط Admin, SeniorUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SeniorUser")]
        public async Task<IActionResult> Edit(int id, WarehouseDto warehouseDto)
        {
            try
            {
                if (id != warehouseDto.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    var success = await _apiService.PutAsync<bool>($"api/Warehouses/{id}", warehouseDto);
                    if (success)
                    {
                        TempData["Success"] = "انبار با موفقیت ویرایش شد";
                        return RedirectToAction(nameof(Index));
                    }
                }
                TempData["Error"] = "خطا در ویرایش انبار";
                return View(warehouseDto);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در ویرایش انبار: {ex.Message}");
                TempData["Error"] = "خطا در ویرایش انبار";
                return View(warehouseDto);
            }
        }

        // POST: Warehouses/Delete/5 - فقط Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _apiService.DeleteAsync($"api/Warehouses/{id}");
                if (success)
                {
                    TempData["Success"] = "انبار با موفقیت حذف شد";
                }
                else
                {
                    TempData["Error"] = "خطا در حذف انبار";
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در حذف انبار: {ex.Message}");
                TempData["Error"] = "خطا در حذف انبار";
            }

            return RedirectToAction(nameof(Index));
        }
    }

    public class WarehouseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateWarehouseDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
    }
}