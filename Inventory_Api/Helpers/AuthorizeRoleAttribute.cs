using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Inventory_Api.Data;

namespace Inventory_Api.Helpers
{
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var roleIdClaim = user.Claims.FirstOrDefault(c => c.Type == "RoleId");
            if (roleIdClaim == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var roleId = int.Parse(roleIdClaim.Value);
            var roleName = Roles.GetRoleName(roleId);

            if (!_allowedRoles.Contains(roleName))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}