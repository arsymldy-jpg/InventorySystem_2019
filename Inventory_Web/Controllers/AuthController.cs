using Inventory_Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Inventory_Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApiService _apiService;

        public AuthController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login(string personnelCode, string password)
        {
            try
            {
                System.Console.WriteLine($"🔐 درخواست لاگین برای کاربر: {personnelCode}");

                var loginRequest = new { PersonnelCode = personnelCode, Password = password };
                var result = await _apiService.PostAsync<AuthResponse>("api/Auth/login", loginRequest, false);

                if (result != null && !string.IsNullOrEmpty(result.Token))
                {
                    System.Console.WriteLine("✅ توکن دریافت شد، ایجاد session و احراز هویت...");

                    // ذخیره توکن در سشن
                    HttpContext.Session.SetString("Token", result.Token);

                    // ذخیره توکن در ViewBag برای انتقال به صفحه
                    ViewBag.Token = result.Token;

                    // یا در TempData
                    TempData["Token"] = result.Token;

                    // ایجاد claims برای احراز هویت Cookie
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, result.User.Id.ToString()),
                        new Claim(ClaimTypes.Name, result.User.PersonnelCode),
                        new Claim(ClaimTypes.Role, result.User.RoleName)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    // احراز هویت
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTime.UtcNow.AddHours(8)
                        });

                    System.Console.WriteLine("✅ احراز هویت انجام شد، هدایت به داشبورد...");
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Error = "کد پرسنلی یا رمز عبور اشتباه است";
                return View();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"❌ خطا در لاگین: {ex.Message}");
                ViewBag.Error = "خطا در ارتباط با سرور";
                return View();
            }
        }

        // در AuthController.cs - اضافه کردن این متد
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }
    }

    public class AuthResponse
    {
        public string Token { get; set; }
        public UserDto User { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PersonnelCode { get; set; }
        public string RoleName { get; set; }
    }
}