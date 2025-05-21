using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class ProjectMember
    {
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.Now;
        public ProjectRole Role { get; set; } = ProjectRole.Member;
    }

    public enum ProjectRole
    {
        Owner,
        Manager,
        Member
    }
} 