using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjectManagementSystem.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Required]
        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }

        public ICollection<ProjectMember> ProjectMembers { get; set; }
        [JsonIgnore]
        public ICollection<ProjectTask> Tasks { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Pending;
    }

    public enum ProjectStatus
    {
        Pending,
        InProgress,
        Completed,
        OnHold,
        Cancelled
    }
} 