using Inventory_Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory_Web.Controllers
{
    [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper")]
    public class CostCentersController : Controller
    {
        private readonly IApiService _apiService;

        public CostCentersController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: CostCenters - مشاهده لیست مراکز هزینه
        public async Task<IActionResult> Index()
        {
            try
            {
                var costCenters = await _apiService.GetAsync<List<CostCenterItemDto>>("api/CostCenters");
                return View(costCenters ?? new List<CostCenterItemDto>());
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت مراکز هزینه: {ex.Message}");
                TempData["Error"] = "خطا در دریافت لیست مراکز هزینه";
                return View(new List<CostCenterItemDto>());
            }
        }

        // GET: CostCenters/Create - فرم ایجاد مرکز هزینه جدید
        public IActionResult Create()
        {
            return View();
        }

        // POST: CostCenters/Create - ثبت مرکز هزینه جدید
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCostCenterViewModel createCostCenterDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _apiService.PostAsync<CostCenterItemDto>("api/CostCenters", createCostCenterDto);
                    if (result != null)
                    {
                        TempData["Success"] = "مرکز هزینه با موفقیت ایجاد شد";
                        return RedirectToAction(nameof(Index));
                    }
                }
                TempData["Error"] = "خطا در ایجاد مرکز هزینه";
                return View(createCostCenterDto);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در ایجاد مرکز هزینه: {ex.Message}");
                TempData["Error"] = "خطا در ایجاد مرکز هزینه";
                return View(createCostCenterDto);
            }
        }

        // GET: CostCenters/Edit/5 - فرم ویرایش مرکز هزینه
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var costCenter = await _apiService.GetAsync<CostCenterItemDto>($"api/CostCenters/{id}");
                if (costCenter == null)
                {
                    return NotFound();
                }
                return View(costCenter);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت اطلاعات مرکز هزینه: {ex.Message}");
                TempData["Error"] = "خطا در دریافت اطلاعات مرکز هزینه";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: CostCenters/Edit/5 - ثبت ویرایش مرکز هزینه
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CostCenterItemDto costCenterDto)
        {
            try
            {
                if (id != costCenterDto.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    var success = await _apiService.PutAsync<bool>($"api/CostCenters/{id}", costCenterDto);
                    if (success)
                    {
                        TempData["Success"] = "مرکز هزینه با موفقیت ویرایش شد";
                        return RedirectToAction(nameof(Index));
                    }
                }
                TempData["Error"] = "خطا در ویرایش مرکز هزینه";
                return View(costCenterDto);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در ویرایش مرکز هزینه: {ex.Message}");
                TempData["Error"] = "خطا در ویرایش مرکز هزینه";
                return View(costCenterDto);
            }
        }
    }

    // مدل‌های جدید با نام‌های منحصربفرد
    public class CostCenterItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }

    public class CreateCostCenterViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}