namespace Inventory_Web.Utilities
{
    public static class Constants
    {
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string SeniorUser = "SeniorUser";
            public const string SeniorStorekeeper = "SeniorStorekeeper";
            public const string Storekeeper = "Storekeeper";
            public const string Viewer = "Viewer";
        }

        public static class ApiEndpoints
        {
            public const string Login = "/api/Auth/login";
            public const string Verify = "/api/Auth/verify";
            public const string Users = "/api/Users";
            public const string Products = "/api/Products";
            public const string Warehouses = "/api/Warehouses";
            public const string Inventory = "/api/Inventory";
            public const string CostCenters = "/api/CostCenters";
            public const string Brands = "/api/Brands";
            public const string Audit = "/api/Audit";
            public const string Reports = "/api/Reports";
        }

        public static class Messages
        {
            public const string LoginFailed = "کد پرسنلی یا رمز عبور اشتباه است";
            public const string AccountExpired = "حساب کاربری شما منقضی شده است";
            public const string AccessDenied = "شما دسترسی به این بخش را ندارید";
        }
    }
}