using System.ComponentModel.DataAnnotations;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserRole Role { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Role")]
        public UserRole Role { get; set; }

        public List<RoleViewModel> Roles { get; set; } = new List<RoleViewModel>();
    }

    public class RoleViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
} 