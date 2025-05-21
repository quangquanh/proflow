using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProjectManagementSystem.Models
{
    public class TaskComment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Required]
        public int TaskId { get; set; }
        
        [ForeignKey("TaskId")]
        [JsonIgnore]
        public ProjectTask Task { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        
        public ICollection<CommentMention> Mentions { get; set; } = new List<CommentMention>();
    }
} 