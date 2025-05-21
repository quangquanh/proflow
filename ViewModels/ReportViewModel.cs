using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.ViewModels
{
    public class ReportViewModel
    {
        public Dictionary<string, int> TasksByStatus { get; set; }
        public Dictionary<string, int> TasksByPriority { get; set; }
        public Dictionary<string, int> TasksPerUser { get; set; }
        public IEnumerable<object> WeeklyCompletionTrend { get; set; }
        public IEnumerable<object> MonthlyCompletionTrend { get; set; }
    }
    
    public class ProjectReportViewModel
    {
        public Project Project { get; set; }
        public Dictionary<string, int> TasksByStatus { get; set; }
        public Dictionary<string, int> TasksByPriority { get; set; }
        public Dictionary<string, int> TasksPerUser { get; set; }
        public IEnumerable<object> WeeklyCompletionTrend { get; set; }
    }
    
    public class UserReportViewModel
    {
        public ApplicationUser User { get; set; }
        public IEnumerable<ProjectTask> Tasks { get; set; }
        public Dictionary<string, int> TasksByStatus { get; set; }
        public Dictionary<string, int> TasksByPriority { get; set; }
        public Dictionary<string, int> TasksByProject { get; set; }
        public IEnumerable<object> WeeklyCompletionTrend { get; set; }
        public IEnumerable<ApplicationUser> Users { get; set; }
        public bool IsAdmin { get; set; }
    }
} 