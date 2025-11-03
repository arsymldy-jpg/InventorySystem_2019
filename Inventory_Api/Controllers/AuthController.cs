using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Inventory_Api.Data;
using Inventory_Api.Models.Entities;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Services;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasherService _passwordHasher;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthController(
            ApplicationDbContext context,
            PasswordHasherService passwordHasher,
            JwtService jwtService,
            IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            // پیدا کردن کاربر با کد پرسنلی
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.PersonnelCode == loginDto.PersonnelCode && u.IsActive);

            if (user == null)
            {
                return Unauthorized(new { Message = "کد پرسنلی یا رمز عبور اشتباه است" });
            }

            // بررسی انقضای حساب کاربری
            if (user.ExpiryDate.HasValue && user.ExpiryDate.Value < DateTime.UtcNow)
            {
                return Unauthorized(new { Message = "حساب کاربری شما منقضی شده است" });
            }

            // بررسی رمز عبور
            if (!_passwordHasher.VerifyPassword(user.PasswordHash, loginDto.Password))
            {
                return Unauthorized(new { Message = "کد پرسنلی یا رمز عبور اشتباه است" });
            }

            // به روز رسانی آخرین زمان ورود
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // تولید توکن
            var token = _jwtService.GenerateToken(user);

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

            var response = new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                User = userDto
            };

            return Ok(response);
        }

        [HttpPost("verify")]
        public async Task<ActionResult<UserDto>> VerifyToken()
        {
            // این متد بعداً با JWT Middleware کامل می‌شود
            return Ok(new { Message = "Token verification endpoint" });
        }
    }
}