using ProjectManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.ViewModels
{
    public class CommentViewModel
    {
        public int Id { get; set; }
        
        public string Content { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public string UserId { get; set; }
        
        public string UserName { get; set; }
        
        public string UserProfilePicture { get; set; }
        
        public bool IsCurrentUserComment { get; set; }
        
        public List<MentionViewModel> Mentions { get; set; } = new List<MentionViewModel>();
    }
    
    public class CreateCommentViewModel
    {
        [Required(ErrorMessage = "Comment content is required")]
        public string Content { get; set; }
        
        [Required]
        public int TaskId { get; set; }
        
        public List<string> TaggedUserIds { get; set; } = new List<string>();
    }
    
    public class MentionViewModel
    {
        public string UserId { get; set; }
        
        public string UserName { get; set; }
    }
    
    public class TaskCommentsViewModel
    {
        public int TaskId { get; set; }
        
        public string TaskTitle { get; set; }
        
        public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();
        
        public CreateCommentViewModel NewComment { get; set; }
        
        public List<CommentUserViewModel> ProjectMembers { get; set; } = new List<CommentUserViewModel>();
    }
    
    public class CommentUserViewModel
    {
        public string Id { get; set; }
        
        public string FullName { get; set; }
        
        public string Username { get; set; }
        
        public string ProfilePicture { get; set; }
    }
    
    public class NotificationViewModel
    {
        public int Id { get; set; }
        
        public string Content { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public NotificationType Type { get; set; }
        
        public bool IsRead { get; set; }
        
        public string Link { get; set; }
    }
} 