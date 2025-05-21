using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.API
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Họ là bắt buộc")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Tên là bắt buộc")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }
    }
} 