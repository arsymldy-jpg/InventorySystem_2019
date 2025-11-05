using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory_Web.Services;
using System;

namespace Inventory_Web.Controllers
{
    [Authorize(Roles = "Admin,SeniorUser,SeniorStorekeeper")]
    public class UsersController : Controller
    {
        private readonly IApiService _apiService;

        public UsersController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Users - با توجه به نقش کاربر جاری
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _apiService.GetAsync<List<UserDisplayDto>>("api/Users");
                return View(users ?? new List<UserDisplayDto>());
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت کاربران: {ex.Message}");
                TempData["Error"] = "خطا در دریافت لیست کاربران";
                return View(new List<UserDisplayDto>());
            }
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _apiService.GetAsync<UserDisplayDto>($"api/Users/{id}");
                if (user == null)
                {
                    return NotFound();
                }
                return View(user);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت جزئیات کاربر: {ex.Message}");
                TempData["Error"] = "خطا در دریافت اطلاعات کاربر";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Users/Create - با توجه به نقش کاربر جاری
        public IActionResult Create()
        {
            // لیست نقش‌های مجاز بر اساس کاربر جاری
            ViewBag.AvailableRoles = GetAvailableRoles();
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserDto createUserDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // اعتبارسنجی تاریخ انقضا
                    if (!string.IsNullOrEmpty(createUserDto.ExpiryDate))
                    {
                        if (!DateTime.TryParse(createUserDto.ExpiryDate, out _))
                        {
                            ModelState.AddModelError("ExpiryDate", "فرمت تاریخ نامعتبر است");
                            ViewBag.AvailableRoles = GetAvailableRoles();
                            return View(createUserDto);
                        }
                    }

                    var result = await _apiService.PostAsync<UserDisplayDto>("api/Users", createUserDto);
                    if (result != null)
                    {
                        TempData["Success"] = "کاربر با موفقیت ایجاد شد";
                        return RedirectToAction(nameof(Index));
                    }
                }

                ViewBag.AvailableRoles = GetAvailableRoles();
                TempData["Error"] = "خطا در ایجاد کاربر";
                return View(createUserDto);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در ایجاد کاربر: {ex.Message}");

                // مدیریت خطاهای خاص از API
                if (ex.Message.Contains("VALIDATION_ERROR:"))
                {
                    var errorJson = ex.Message.Replace("VALIDATION_ERROR:", "");
                    if (errorJson.Contains("کد پرسنلی تکراری است"))
                    {
                        TempData["Error"] = "کد پرسنلی تکراری است. لطفاً کد دیگری انتخاب کنید.";
                    }
                    else
                    {
                        TempData["Error"] = "خطا در ایجاد کاربر: " + errorJson;
                    }
                }
                else
                {
                    TempData["Error"] = "خطا در ایجاد کاربر";
                }

                ViewBag.AvailableRoles = GetAvailableRoles();
                return View(createUserDto);
            }
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _apiService.GetAsync<UserDisplayDto>($"api/Users/{id}");
                if (user == null)
                {
                    return NotFound();
                }

                // تبدیل UserDisplayDto به UserEditDto
                var userEditDto = new UserEditDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Mobile = user.Mobile,
                    Email = user.Email,
                    ExpiryDate = user.ExpiryDate
                };

                return View(userEditDto);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در دریافت اطلاعات برای ویرایش: {ex.Message}");
                TempData["Error"] = "خطا در دریافت اطلاعات کاربر";
                return RedirectToAction(nameof(Index));
            }
        }


        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditDto userEditDto)
        {
            try
            {
                if (id != userEditDto.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    // ایجاد مدل مطابق با انتظار API
                    var updateData = new
                    {
                        Id = userEditDto.Id,
                        FirstName = userEditDto.FirstName,
                        LastName = userEditDto.LastName,
                        Mobile = userEditDto.Mobile,
                        Email = userEditDto.Email,
                        ExpiryDate = userEditDto.ExpiryDate
                    };

                    var success = await _apiService.PutAsync<bool>($"api/Users/{id}", updateData);
                    if (success)
                    {
                        TempData["Success"] = "کاربر با موفقیت ویرایش شد";
                        return RedirectToAction(nameof(Index));
                    }
                }
                TempData["Error"] = "خطا در ویرایش کاربر";
                return View(userEditDto);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در ویرایش کاربر: {ex.Message}");
                TempData["Error"] = "خطا در ویرایش کاربر";
                return View(userEditDto);
            }
        }


        // POST: Users/Deactivate/5 - غیرفعال کردن کاربر
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                var success = await _apiService.PutAsync<bool>($"api/Users/{id}/deactivate", new { });
                if (success)
                {
                    TempData["Success"] = "کاربر با موفقیت غیرفعال شد";
                }
                else
                {
                    TempData["Error"] = "خطا در غیرفعال کردن کاربر";
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در غیرفعال کردن کاربر: {ex.Message}");
                TempData["Error"] = "خطا در غیرفعال کردن کاربر";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Users/Delete/5 - فقط Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _apiService.DeleteAsync($"api/Users/{id}");
                if (success)
                {
                    TempData["Success"] = "کاربر با موفقیت حذف شد";
                }
                else
                {
                    TempData["Error"] = "خطا در حذف کاربر";
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در حذف کاربر: {ex.Message}");
                TempData["Error"] = "خطا در حذف کاربر";
            }

            return RedirectToAction(nameof(Index));
        }

        // تعیین نقش‌های مجاز بر اساس کاربر جاری
        private List<RoleItem> GetAvailableRoles()
        {
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            return currentUserRole switch
            {
                "Admin" => new List<RoleItem>
                {
                    new RoleItem { Id = 1, Name = "Admin" },
                    new RoleItem { Id = 2, Name = "SeniorUser" },
                    new RoleItem { Id = 3, Name = "SeniorStorekeeper" },
                    new RoleItem { Id = 4, Name = "Storekeeper" },
                    new RoleItem { Id = 5, Name = "Viewer" }
                },
                "SeniorUser" => new List<RoleItem>
                {
                    new RoleItem { Id = 3, Name = "SeniorStorekeeper" },
                    new RoleItem { Id = 4, Name = "Storekeeper" },
                    new RoleItem { Id = 5, Name = "Viewer" }
                },
                "SeniorStorekeeper" => new List<RoleItem>
                {
                    new RoleItem { Id = 4, Name = "Storekeeper" }
                },
                _ => new List<RoleItem>()
            };
        }
    }

    public class UserDisplayDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PersonnelCode { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public string ExpiryDate { get; set; }
        public string CreatedDate { get; set; }
        public string LastLogin { get; set; }
    }

    // اضافه کردن این کلاس به UsersController.cs
    public class UserEditDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string ExpiryDate { get; set; }
    }


    public class CreateUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PersonnelCode { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public string ExpiryDate { get; set; }
    }

    public class RoleItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}