using System.ComponentModel.DataAnnotations;

namespace Inventory_Web.Models.ApiModels
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "کد پرسنلی الزامی است")]
        public string PersonnelCode { get; set; }

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        public string Password { get; set; }
    }
}