using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Inventory_Web.Services
{
    public class ApiService : IApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public ApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        private HttpClient CreateHttpClient(bool requiresAuth = true)
        {
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:44342";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);

            if (requiresAuth)
            {
                // روش مطمئن‌تر: خواندن توکن از session
                var token = _httpContextAccessor.HttpContext?.Session?.GetString("Token");

                if (string.IsNullOrEmpty(token))
                {
                    // روش جایگزین: چک کردن وجود کاربر احراز هویت شده
                    Console.WriteLine("⚠️ توکن در session یافت نشد");
                }
                else
                {
                    Console.WriteLine($"✅ توکن پیدا شد: {token.Substring(0, Math.Min(20, token.Length))}...");
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }

            return client;
        }

        //public async Task<T> GetAsync<T>(string endpoint, bool requiresAuth = true)
        //{
        //    try
        //    {
        //        using var client = CreateHttpClient(requiresAuth);
        //        Console.WriteLine($"🔍 ارسال درخواست GET به: {endpoint}");

        //        var response = await client.GetAsync(endpoint);
        //        Console.WriteLine($"📡 وضعیت پاسخ: {response.StatusCode}");

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            Console.WriteLine($"❌ خطای HTTP: {response.StatusCode}");
        //            var errorContent = await response.Content.ReadAsStringAsync();
        //            Console.WriteLine($"❌ محتوای خطا: {errorContent}");
        //        }

        //        return await HandleResponse<T>(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"💥 خطا در GetAsync: {ex.Message}");
        //        throw;
        //    }
        //}


        public async Task<T> GetAsync<T>(string endpoint, bool requiresAuth = true)
        {
            try
            {
                using var client = CreateHttpClient(requiresAuth);
                System.Console.WriteLine($"🔍 ارسال درخواست GET به: {endpoint}");

                var response = await client.GetAsync(endpoint);
                System.Console.WriteLine($"📡 وضعیت پاسخ: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    System.Console.WriteLine($"❌ خطای HTTP: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"❌ محتوای خطا: {errorContent}");
                }

                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"💥 خطا در GetAsync: {ex.Message}");
                System.Console.WriteLine($"💥 StackTrace: {ex.StackTrace}");
                throw;
            }
        }


        public async Task<T> PostAsync<T>(string endpoint, object data, bool requiresAuth = true)
        {
            try
            {
                using var client = CreateHttpClient(requiresAuth);

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                Console.WriteLine($"🔍 ارسال درخواست POST به: {endpoint}");
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);

                Console.WriteLine($"📡 وضعیت پاسخ: {response.StatusCode}");

                // مدیریت خطاهای خاص
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ محتوای خطا: {errorContent}");

                    // اگر خطای validation از API باشد
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException($"VALIDATION_ERROR:{errorContent}");
                    }
                }

                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 خطا در PostAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<T> PutAsync<T>(string endpoint, object data, bool requiresAuth = true)
        {
            try
            {
                using var client = CreateHttpClient(requiresAuth);

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                Console.WriteLine($"🔍 ارسال درخواست PUT به: {endpoint}");
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync(endpoint, content);

                Console.WriteLine($"📡 وضعیت پاسخ: {response.StatusCode}");

                // مدیریت خطاهای خاص
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ محتوای خطا: {errorContent}");

                    // اگر خطای validation از API باشد
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException($"VALIDATION_ERROR:{errorContent}");
                    }
                }

                // 🔥 تغییر: اگر وضعیت 204 بود، true برگردان
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    Console.WriteLine("✅ درخواست PUT موفق (204 No Content)");
                    if (typeof(T) == typeof(bool))
                    {
                        return (T)(object)true;
                    }
                    return default(T);
                }

                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 خطا در PutAsync: {ex.Message}");
                throw;
            }
        }


        public async Task<bool> DeleteAsync(string endpoint, bool requiresAuth = true)
        {
            try
            {
                using var client = CreateHttpClient(requiresAuth);
                Console.WriteLine($"🔍 ارسال درخواست DELETE به: {endpoint}");

                var response = await client.DeleteAsync(endpoint);
                Console.WriteLine($"📡 وضعیت پاسخ: {response.StatusCode}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 خطا در DeleteAsync: {ex.Message}");
                return false;
            }
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            // 🔥 تغییر: اگر وضعیت 204 بود، true برگردان
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                Console.WriteLine("✅ درخواست موفق (204 No Content)");
                if (typeof(T) == typeof(bool))
                {
                    return (T)(object)true;
                }
                return default(T);
            }

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    if (string.IsNullOrEmpty(content))
                        return default(T);

                    return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"❌ خطای JSON: {ex.Message}");
                    return default(T);
                }
            }
            else
            {
                Console.WriteLine($"❌ درخواست ناموفق: {response.StatusCode}");
                Console.WriteLine($"❌ پاسخ: {content}");
                throw new HttpRequestException($"خطای API: {response.StatusCode} - {content}");
            }
        }

        public async Task<string> TestAuth()
        {
            try
            {
                using var client = CreateHttpClient(true);
                var response = await client.GetAsync("api/Profile");

                if (response.IsSuccessStatusCode)
                {
                    return "Auth SUCCESS";
                }
                else
                {
                    return $"Auth FAILED: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"Auth ERROR: {ex.Message}";
            }
        }
    }
}