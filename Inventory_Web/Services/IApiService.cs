// Services/IApiService.cs
using System.Threading.Tasks;

namespace Inventory_Web.Services
{
    public interface IApiService
    {
        Task<T> GetAsync<T>(string endpoint, bool requiresAuth = true);
        Task<T> PostAsync<T>(string endpoint, object data, bool requiresAuth = true);
        Task<T> PutAsync<T>(string endpoint, object data, bool requiresAuth = true);
        Task<bool> DeleteAsync(string endpoint, bool requiresAuth = true);
        Task<string> TestAuth();
    }
}