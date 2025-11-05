using Inventory_Web.Models.ApiModels;
using Inventory_Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

public class SimpleAuthService
{
    private readonly IApiService _apiService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SimpleAuthService(IApiService apiService, IHttpContextAccessor httpContextAccessor)
    {
        _apiService = apiService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> LoginAsync(string personnelCode, string password)
    {
        try
        {
            var loginRequest = new LoginRequest
            {
                PersonnelCode = personnelCode,
                Password = password
            };

            var response = await _apiService.PostAsync<AuthResponse>(
                "api/Auth/login",
                loginRequest,
                requiresAuth: false
            );

            if (response?.User != null)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, response.User.Id.ToString()),
                    new Claim(ClaimTypes.Name, response.User.PersonnelCode),
                    new Claim(ClaimTypes.Role, response.User.RoleName)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await _httpContextAccessor.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);

                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}