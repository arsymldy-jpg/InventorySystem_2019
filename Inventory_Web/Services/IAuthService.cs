using System.Threading.Tasks;
using Inventory_Web.Models.ApiModels;

namespace Inventory_Web.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(LoginRequest loginRequest);
        Task LogoutAsync();
        Task<UserDto> GetCurrentUserAsync();
        bool IsAuthenticated();
    }
}