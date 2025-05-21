using System.ComponentModel.DataAnnotations;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.ViewModels
{
    public class ProjectViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Project Name")]
        public string Name { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Status")]
        public ProjectStatus Status { get; set; }
    }

    public class ProjectMemberViewModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public ProjectRole Role { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class AddProjectMemberViewModel
    {
        public int ProjectId { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "User Email")]
        public string UserEmail { get; set; }

        [Display(Name = "Role")]
        public ProjectRole Role { get; set; }
    }
} 