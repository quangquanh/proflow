using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProjectManagementSystem.Models
{
    public enum ProjectTaskStatus
    {
        NotStarted,
        InProgress,
        Completed,
        OnHold
    }

    public enum Priority
    {
        Low,
        Medium,
        High
    }

    public class ProjectTask
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        
        public string? Description { get; set; }
        
        [Required]
        public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.NotStarted;
        
        [Required]
        public Priority Priority { get; set; } = Priority.Medium;
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Required]
        public DateTime Deadline { get; set; }
        
        public string? AttachmentPath { get; set; }
        
        [Required]
        public int ProjectId { get; set; }
        
        [ForeignKey("ProjectId")]
        [JsonIgnore]
        public Project Project { get; set; }
        
        public string CreatedById { get; set; }
        
        [ForeignKey("CreatedById")]
        public ApplicationUser CreatedBy { get; set; }
        
        public string? AssignedToId { get; set; }
        
        [ForeignKey("AssignedToId")]
        public ApplicationUser AssignedTo { get; set; }
        
        public bool ReminderSent { get; set; } = false;
        
        // Collection of comments associated with this task
        public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    }
} 