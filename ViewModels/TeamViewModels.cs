using System.ComponentModel.DataAnnotations;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.ViewModels
{
    public class TeamViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public List<TeamMemberViewModel> TeamMembers { get; set; } = new List<TeamMemberViewModel>();
    }

    public class TeamMemberViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public TeamRole Role { get; set; }
    }

    public class AddTeamMemberViewModel
    {
        public int TeamId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public TeamRole Role { get; set; }
    }
} 