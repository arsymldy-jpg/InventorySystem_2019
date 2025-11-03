using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Helpers;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper, Roles.Storekeeper, Roles.Viewer)]
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Profile
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Id == int.Parse(userId) && u.IsActive)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    PersonnelCode = u.PersonnelCode,
                    Mobile = u.Mobile,
                    Email = u.Email,
                    RoleName = u.Role.Name,
                    IsActive = u.IsActive,
                    ExpiryDate = u.ExpiryDate,
                    CreatedDate = u.CreatedDate,
                    LastLogin = u.LastLogin
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Profile
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto updateProfileDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId) && u.IsActive);

            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = updateProfileDto.FirstName;
            user.LastName = updateProfileDto.LastName;
            user.Mobile = updateProfileDto.Mobile;
            user.Email = updateProfileDto.Email;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Profile/permissions
        [HttpGet("permissions")]
        public ActionResult<object> GetPermissions()
        {
            var roleId = User.FindFirst("RoleId")?.Value;
            var roleName = Roles.GetRoleName(int.Parse(roleId));

            return new
            {
                Role = roleName,
                Permissions = GetRolePermissions(roleName),
                UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                PersonnelCode = User.FindFirst(ClaimTypes.Name)?.Value
            };
        }

        private object GetRolePermissions(string roleName)
        {
            return roleName switch
            {
                Roles.Admin => new
                {
                    CanManageUsers = true,
                    CanManageAllWarehouses = true,
                    CanManageProducts = true,
                    CanManageInventory = true,
                    CanViewReports = true
                },
                Roles.SeniorUser => new
                {
                    CanManageUsers = true, // فقط کاربران پایین‌تر
                    CanManageAllWarehouses = true,
                    CanManageProducts = true,
                    CanManageInventory = true,
                    CanViewReports = true
                },
                Roles.SeniorStorekeeper => new
                {
                    CanManageUsers = true, // فقط انبارداران
                    CanManageAllWarehouses = true,
                    CanManageProducts = true,
                    CanManageInventory = true,
                    CanViewReports = true
                },
                Roles.Storekeeper => new
                {
                    CanManageUsers = false,
                    CanManageAllWarehouses = false, // فقط انبارهای زیرمجموعه
                    CanManageProducts = false,
                    CanManageInventory = true, // فقط انبارهای زیرمجموعه
                    CanViewReports = true
                },
                Roles.Viewer => new
                {
                    CanManageUsers = false,
                    CanManageAllWarehouses = false,
                    CanManageProducts = false,
                    CanManageInventory = false,
                    CanViewReports = true
                },
                _ => new
                {
                    CanManageUsers = false,
                    CanManageAllWarehouses = false,
                    CanManageProducts = false,
                    CanManageInventory = false,
                    CanViewReports = false
                }
            };
        }
    }
}