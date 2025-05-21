using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public enum NotificationType
    {
        Mention,
        Reply,
        TaskAssignment,
        TaskDue,
        TaskStatusChanged,
        CommentAdded,
        ProjectMemberAdded
    }

    public class Notification
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Required]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        
        [Required]
        public NotificationType Type { get; set; }
        
        public int? TaskId { get; set; }
        
        [ForeignKey("TaskId")]
        public ProjectTask Task { get; set; }
        
        public int? CommentId { get; set; }
        
        [ForeignKey("CommentId")]
        public TaskComment Comment { get; set; }
        
        [Required]
        public bool IsRead { get; set; } = false;
        
        // URL to navigate to when notification is clicked
        public string Link { get; set; }
    }
} 