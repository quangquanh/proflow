using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class CommentMention
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int CommentId { get; set; }
        
        [ForeignKey("CommentId")]
        public TaskComment Comment { get; set; }
        
        [Required]
        public string MentionedUserId { get; set; }
        
        [ForeignKey("MentionedUserId")]
        public ApplicationUser MentionedUser { get; set; }
        
        [Required]
        public bool IsRead { get; set; } = false;
    }
} 