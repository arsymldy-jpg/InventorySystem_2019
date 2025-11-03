using System;

namespace Inventory_Api.Helpers
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string SeniorUser = "SeniorUser";
        public const string SeniorStorekeeper = "SeniorStorekeeper";
        public const string Storekeeper = "Storekeeper";
        public const string Viewer = "Viewer";

        public static int GetRoleId(string roleName)
        {
            return roleName switch
            {
                Admin => 1,
                SeniorUser => 2,
                SeniorStorekeeper => 3,
                Storekeeper => 4,
                Viewer => 5,
                _ => 5 // پیش‌فرض Viewer
            };
        }

        public static string GetRoleName(int roleId)
        {
            return roleId switch
            {
                1 => Admin,
                2 => SeniorUser,
                3 => SeniorStorekeeper,
                4 => Storekeeper,
                5 => Viewer,
                _ => Viewer
            };
        }
    }
}