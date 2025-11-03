using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Api.Models.DTOs
{
    public class LoginDto
    {
        [Required]
        public string PersonnelCode { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public UserDto User { get; set; }
    }
}