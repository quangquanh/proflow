using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class Team
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }

        public ICollection<TeamMember> TeamMembers { get; set; }
        public ICollection<ProjectTeam> ProjectTeams { get; set; }
    }
} 