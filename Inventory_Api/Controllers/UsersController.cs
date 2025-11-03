using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models.Entities;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Services;
using Inventory_Api.Helpers;
using System.Security.Claims;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper)]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasherService _passwordHasher;

        public UsersController(ApplicationDbContext context, PasswordHasherService passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentUserRoleId = int.Parse(User.FindFirst("RoleId")?.Value);
            var currentUserRole = Roles.GetRoleName(currentUserRoleId);

            IQueryable<User> usersQuery = _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive);

            // فیلتر کردن کاربران بر اساس نقش کاربر جاری
            switch (currentUserRole)
            {
                case Roles.Admin:
                    // ادمین همه کاربران را می‌بیند
                    break;
                case Roles.SeniorUser:
                    // کاربر ارشد فقط کاربران با نقش‌های پایین‌تر را می‌بیند
                    usersQuery = usersQuery.Where(u => u.RoleId > 2); // فقط نقش‌های 3,4,5
                    break;
                case Roles.SeniorStorekeeper:
                    // انباردار ارشد فقط انبارداران را می‌بیند
                    usersQuery = usersQuery.Where(u => u.RoleId == 4); // فقط Storekeeper
                    break;
                default:
                    return Forbid();
            }

            var users = await usersQuery
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
                    CreatedDate = u.CreatedDate
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentUserRoleId = int.Parse(User.FindFirst("RoleId")?.Value);
            var currentUserRole = Roles.GetRoleName(currentUserRoleId);

            var user = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Id == id && u.IsActive)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            // بررسی دسترسی بر اساس نقش
            if (!CanAccessUser(currentUserRole, user.RoleId))
            {
                return Forbid();
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PersonnelCode = user.PersonnelCode,
                Mobile = user.Mobile,
                Email = user.Email,
                RoleName = user.Role.Name,
                IsActive = user.IsActive,
                ExpiryDate = user.ExpiryDate,
                CreatedDate = user.CreatedDate
            };

            return userDto;
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            var currentUserRoleId = int.Parse(User.FindFirst("RoleId")?.Value);
            var currentUserRole = Roles.GetRoleName(currentUserRoleId);

            // بررسی مجوز ایجاد کاربر با نقش مورد نظر
            if (!CanCreateUserWithRole(currentUserRole, createUserDto.RoleId))
            {
                return Forbid("شما مجوز ایجاد کاربر با این نقش را ندارید");
            }

            // بررسی تکراری نبودن کد پرسنلی
            if (await _context.Users.AnyAsync(u => u.PersonnelCode == createUserDto.PersonnelCode))
            {
                return BadRequest(new { Message = "کد پرسنلی تکراری است" });
            }

            var user = new User
            {
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                PersonnelCode = createUserDto.PersonnelCode,
                Mobile = createUserDto.Mobile,
                Email = createUserDto.Email,
                PasswordHash = _passwordHasher.HashPassword(createUserDto.Password),
                RoleId = createUserDto.RoleId,
                IsActive = true,
                ExpiryDate = createUserDto.ExpiryDate,
                CreatedDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PersonnelCode = user.PersonnelCode,
                Mobile = user.Mobile,
                Email = user.Email,
                RoleName = (await _context.Roles.FindAsync(user.RoleId))?.Name,
                IsActive = user.IsActive,
                ExpiryDate = user.ExpiryDate,
                CreatedDate = user.CreatedDate
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDto);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserDto userDto)
        {
            if (id != userDto.Id)
            {
                return BadRequest();
            }

            var currentUserRoleId = int.Parse(User.FindFirst("RoleId")?.Value);
            var currentUserRole = Roles.GetRoleName(currentUserRoleId);

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);

            if (user == null)
            {
                return NotFound();
            }

            // بررسی دسترسی برای ویرایش این کاربر
            if (!CanAccessUser(currentUserRole, user.RoleId))
            {
                return Forbid();
            }

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Mobile = userDto.Mobile;
            user.Email = userDto.Email;
            user.ExpiryDate = userDto.ExpiryDate;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Users/5 - فقط برای Admin
        [HttpDelete("{id}")]
        [AuthorizeRole(Roles.Admin)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Soft Delete - فقط غیرفعال می‌کنیم
            user.IsActive = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Users/5/deactivate - غیرفعال کردن کاربر
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var currentUserRoleId = int.Parse(User.FindFirst("RoleId")?.Value);
            var currentUserRole = Roles.GetRoleName(currentUserRoleId);

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);

            if (user == null)
            {
                return NotFound();
            }

            // بررسی دسترسی برای غیرفعال کردن این کاربر
            if (!CanAccessUser(currentUserRole, user.RoleId))
            {
                return Forbid();
            }

            user.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id && e.IsActive);
        }

        // بررسی دسترسی به کاربر بر اساس نقش
        private bool CanAccessUser(string currentUserRole, int targetUserRoleId)
        {
            return currentUserRole switch
            {
                Roles.Admin => true, // ادمین به همه دسترسی دارد
                Roles.SeniorUser => targetUserRoleId > 2, // کاربر ارشد فقط به نقش‌های پایین‌تر دسترسی دارد
                Roles.SeniorStorekeeper => targetUserRoleId == 4, // انباردار ارشد فقط به انبارداران دسترسی دارد
                _ => false
            };
        }

        // بررسی مجوز ایجاد کاربر با نقش مورد نظر
        private bool CanCreateUserWithRole(string currentUserRole, int targetRoleId)
        {
            return currentUserRole switch
            {
                Roles.Admin => targetRoleId >= 1 && targetRoleId <= 5, // ادمین می‌تواند هر نقشی ایجاد کند
                Roles.SeniorUser => targetRoleId >= 3 && targetRoleId <= 5, // کاربر ارشد فقط می‌تواند نقش‌های 3,4,5 ایجاد کند
                Roles.SeniorStorekeeper => targetRoleId == 4, // انباردار ارشد فقط می‌تواند انباردار ایجاد کند
                _ => false
            };
        }
    }
}