using Microsoft.AspNetCore.Identity;

namespace ProjectManagementSystem.Models
{
    public enum UserRole
    {
        User,
        Admin
    }

    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public string? ProfilePicture { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
    }
} 