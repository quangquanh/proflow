using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.API
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; }
    }
} 