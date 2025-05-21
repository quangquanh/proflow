using ProjectManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.ViewModels
{
    public class TaskSearchViewModel
    {
        // Tìm kiếm cơ bản
        public string? SearchTerm { get; set; }
        
        // Bộ lọc nâng cao
        public int? ProjectId { get; set; }
        public string? AssignedToId { get; set; }
        public ProjectTaskStatus? Status { get; set; }
        public Priority? Priority { get; set; }
        
        // Lọc theo thời gian
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
        
        // Sắp xếp
        public string? SortBy { get; set; }
        public bool SortAscending { get; set; } = true;
        
        // Phân trang
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
        // Kết quả tìm kiếm
        public List<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
        public int TotalTasks { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalTasks / PageSize);
        
        // Danh sách dropdown
        public List<Project> AvailableProjects { get; set; } = new List<Project>();
        public List<ApplicationUser> AvailableUsers { get; set; } = new List<ApplicationUser>();
    }
} 