using Inventory_Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory_Web.Controllers
{
    [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper")]
    public class WarehouseAccessController : Controller
    {
        private readonly IApiService _apiService;

        public WarehouseAccessController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: WarehouseAccess - مدیریت دسترسی انبارداران
        public async Task<IActionResult> Index()
        {
            try
            {
                // دریافت لیست کاربران انباردار
                var storekeepers = await _apiService.GetAsync<List<StorekeeperUserDto>>("api/Users");

                ViewBag.Storekeepers = storekeepers ?? new List<StorekeeperUserDto>();
                return View();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت اطلاعات: {ex.Message}");
                TempData["Error"] = "خطا در دریافت اطلاعات";
                ViewBag.Storekeepers = new List<StorekeeperUserDto>();
                return View();
            }
        }

        // GET: WarehouseAccess/User/5 - مدیریت دسترسی یک کاربر خاص
        public async Task<IActionResult> UserAccess(int userId)
        {
            try
            {
                // دریافت اطلاعات کاربر
                var user = await _apiService.GetAsync<StorekeeperUserDto>($"api/Users/{userId}");
                if (user == null)
                {
                    TempData["Error"] = "کاربر یافت نشد";
                    return RedirectToAction(nameof(Index));
                }

                // دریافت دسترسی‌های فعلی کاربر
                var userAccess = await _apiService.GetAsync<List<UserWarehouseAccessDto>>($"api/WarehouseAccess/user/{userId}");

                // دریافت لیست تمام انبارها
                var allWarehouses = await _apiService.GetAsync<List<AccessWarehouseInfo>>("api/Warehouses");

                var viewModel = new UserAccessManagementViewModel
                {
                    UserId = userId,
                    UserName = user.FullName,
                    PersonnelCode = user.PersonnelCode,
                    CurrentAccess = userAccess ?? new List<UserWarehouseAccessDto>(),
                    AllWarehouses = allWarehouses ?? new List<AccessWarehouseInfo>()
                };

                return View(viewModel);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت دسترسی کاربر: {ex.Message}");
                TempData["Error"] = "خطا در دریافت دسترسی کاربر";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: WarehouseAccess/Create - ایجاد دسترسی جدید
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAccessViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var createDto = new
                    {
                        UserId = model.UserId,
                        WarehouseId = model.WarehouseId,
                        CanEdit = model.CanEdit,
                        CanView = model.CanView
                    };

                    var result = await _apiService.PostAsync<object>("api/WarehouseAccess", createDto);
                    if (result != null)
                    {
                        TempData["Success"] = "دسترسی با موفقیت ایجاد شد";
                    }
                    else
                    {
                        TempData["Error"] = "خطا در ایجاد دسترسی";
                    }
                }
                return RedirectToAction("UserAccess", new { userId = model.UserId });
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در ایجاد دسترسی: {ex.Message}");
                TempData["Error"] = "خطا در ایجاد دسترسی";
                return RedirectToAction("UserAccess", new { userId = model.UserId });
            }
        }

        // POST: WarehouseAccess/Update - به روزرسانی دسترسی
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateAccessViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var updateDto = new
                    {
                        CanEdit = model.CanEdit,
                        CanView = model.CanView
                    };

                    var success = await _apiService.PutAsync<bool>($"api/WarehouseAccess/{model.AccessId}", updateDto);
                    if (success)
                    {
                        TempData["Success"] = "دسترسی با موفقیت به روزرسانی شد";
                    }
                    else
                    {
                        TempData["Error"] = "خطا در به روزرسانی دسترسی";
                    }
                }
                return RedirectToAction("UserAccess", new { userId = model.UserId });
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در به روزرسانی دسترسی: {ex.Message}");
                TempData["Error"] = "خطا در به روزرسانی دسترسی";
                return RedirectToAction("UserAccess", new { userId = model.UserId });
            }
        }

        // POST: WarehouseAccess/Delete/5 - حذف دسترسی
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int userId)
        {
            try
            {
                var success = await _apiService.DeleteAsync($"api/WarehouseAccess/{id}");
                if (success)
                {
                    TempData["Success"] = "دسترسی با موفقیت حذف شد";
                }
                else
                {
                    TempData["Error"] = "خطا در حذف دسترسی";
                }
                return RedirectToAction("UserAccess", new { userId = userId });
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در حذف دسترسی: {ex.Message}");
                TempData["Error"] = "خطا در حذف دسترسی";
                return RedirectToAction("UserAccess", new { userId = userId });
            }
        }
    }

    // مدل‌های مورد نیاز
    public class StorekeeperUserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PersonnelCode { get; set; }
        public string RoleName { get; set; }
    }

    public class UserWarehouseAccessDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public bool CanEdit { get; set; }
        public bool CanView { get; set; }
    }

    public class AccessWarehouseInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class UserAccessManagementViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string PersonnelCode { get; set; }
        public List<UserWarehouseAccessDto> CurrentAccess { get; set; }
        public List<AccessWarehouseInfo> AllWarehouses { get; set; }
        public CreateAccessViewModel NewAccess { get; set; } = new CreateAccessViewModel();
    }

    public class CreateAccessViewModel
    {
        public int UserId { get; set; }
        public int WarehouseId { get; set; }
        public bool CanEdit { get; set; }
        public bool CanView { get; set; } = true;
    }

    public class UpdateAccessViewModel
    {
        public int AccessId { get; set; }
        public int UserId { get; set; }
        public bool CanEdit { get; set; }
        public bool CanView { get; set; }
    }
}