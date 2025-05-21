using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class TeamMember
    {
        public int Id { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public TeamRole Role { get; set; }

        public DateTime JoinedAt { get; set; }
    }

    public enum TeamRole
    {
        Member,
        TeamLead,
        ProjectManager
    }
} 