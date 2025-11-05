using Microsoft.AspNetCore.Mvc;
using Inventory_Web.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;

namespace Inventory_Web.Controllers
{
    public class TestController : Controller
    {
        private readonly IApiService _apiService;

        public TestController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> CheckSession()
        {
            var token = HttpContext.Session.GetString("Token");
            var hasToken = !string.IsNullOrEmpty(token);

            ViewBag.HasToken = hasToken;
            ViewBag.TokenPreview = hasToken ? token.Substring(0, Math.Min(20, token.Length)) + "..." : "ندارد";
            ViewBag.SessionId = HttpContext.Session.Id;

            // تست ارتباط با API
            ViewBag.ApiTest = await _apiService.TestAuth();

            return View();
        }
    }
}