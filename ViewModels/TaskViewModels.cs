using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.ViewModels
{
    public class TaskViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Title")]
        public string Title { get; set; }
        
        [Display(Name = "Description")]
        public string Description { get; set; }
        
        [Display(Name = "Status")]
        public ProjectTaskStatus Status { get; set; }
        
        [Display(Name = "Priority")]
        public Priority Priority { get; set; }
        
        [Display(Name = "Created At")]
        [DisplayFormat(DataFormatString = "{0:MMM dd, yyyy}")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Deadline")]
        [DisplayFormat(DataFormatString = "{0:MMM dd, yyyy}")]
        public DateTime Deadline { get; set; }
        
        [Display(Name = "Project")]
        public string ProjectName { get; set; }
        
        public int ProjectId { get; set; }
        
        [Display(Name = "Created By")]
        public string CreatedByName { get; set; }
        
        [Display(Name = "Assigned To")]
        public string AssignedToName { get; set; }
        
        public string AssignedToId { get; set; }
        
        public string AttachmentPath { get; set; }
        
        public bool HasAttachment => !string.IsNullOrEmpty(AttachmentPath);
        
        public int CommentCount { get; set; }
    }
    
    public class CreateTaskViewModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Title")]
        public string Title { get; set; }
        
        [Display(Name = "Description")]
        public string Description { get; set; }
        
        [Required]
        [Display(Name = "Status")]
        public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.NotStarted;
        
        [Required]
        [Display(Name = "Priority")]
        public Priority Priority { get; set; } = Priority.Medium;
        
        [Required]
        [Display(Name = "Deadline")]
        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; } = DateTime.Now.AddDays(7);
        
        [Required]
        [Display(Name = "Project")]
        public int ProjectId { get; set; }
        
        [Display(Name = "Assigned To")]
        public string AssignedToId { get; set; }
        
        public IFormFile Attachment { get; set; }
        
        public SelectList Projects { get; set; }
        
        public SelectList ProjectMembers { get; set; }
    }
    
    public class EditTaskViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        [Display(Name = "Title")]
        public string Title { get; set; }
        
        [Display(Name = "Description")]
        public string Description { get; set; }
        
        [Required]
        [Display(Name = "Status")]
        public ProjectTaskStatus Status { get; set; }
        
        [Required]
        [Display(Name = "Priority")]
        public Priority Priority { get; set; }
        
        [Required]
        [Display(Name = "Deadline")]
        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; }
        
        [Required]
        [Display(Name = "Project")]
        public int ProjectId { get; set; }
        
        [Display(Name = "Assigned To")]
        public string AssignedToId { get; set; }
        
        // Optional attachment
        [Display(Name = "Attachment (Optional)")]
        public IFormFile? Attachment { get; set; }
        
        public string ExistingAttachmentPath { get; set; }
        
        public bool HasAttachment => !string.IsNullOrEmpty(ExistingAttachmentPath);
        
        public SelectList Projects { get; set; }
        
        public SelectList ProjectMembers { get; set; }
    }
    
    public class TasksListViewModel
    {
        public List<TaskViewModel> NotStartedTasks { get; set; } = new List<TaskViewModel>();
        public List<TaskViewModel> InProgressTasks { get; set; } = new List<TaskViewModel>();
        public List<TaskViewModel> CompletedTasks { get; set; } = new List<TaskViewModel>();
        public List<TaskViewModel> OnHoldTasks { get; set; } = new List<TaskViewModel>();
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
    }
    
    public class TaskCalendarViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public List<TaskViewModel> Tasks { get; set; } = new List<TaskViewModel>();
        public string ViewMode { get; set; } = "month"; // month, week, day
    }
    
    public class UpdateTaskStatusViewModel
    {
        public int TaskId { get; set; }
        public ProjectTaskStatus NewStatus { get; set; }
    }
} 