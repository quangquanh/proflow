using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class ExternalLoginModel
    {
        [Required]
        public string Provider { get; set; }
        
        [Required]
        public string Email { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public string PhotoUrl { get; set; }
    }
} 