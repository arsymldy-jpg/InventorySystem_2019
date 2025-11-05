// ایجاد مدل برای صفحه دیباگ
// Models/DebugAuthModel.cs
namespace Inventory_Web.Models
{
    public class DebugAuthModel
    {
        public bool IsAuthenticated { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
        public bool TokenExists { get; set; }
        public string TokenPreview { get; set; }
        public string ApiTestResult { get; set; }
    }
}