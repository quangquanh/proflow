using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.ViewModels;
using System.Globalization;

namespace ProjectManagementSystem.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Report/Index
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            
            // Get projects the user has access to
            var projectsQuery = _context.Projects
                .Include(p => p.Tasks)
                .AsQueryable();

            if (!isAdmin)
            {
                projectsQuery = projectsQuery.Where(p => 
                    p.CreatedById == currentUser.Id || 
                    p.ProjectMembers.Any(pm => pm.UserId == currentUser.Id));
            }

            var projects = await projectsQuery.ToListAsync();
            
            // Get task statistics by status
            var tasksByStatus = new Dictionary<string, int>
            {
                { "Not Started", 0 },
                { "In Progress", 0 },
                { "Completed", 0 },
                { "On Hold", 0 }
            };
            
            // Get task statistics by priority
            var tasksByPriority = new Dictionary<string, int>
            {
                { "Low", 0 },
                { "Medium", 0 },
                { "High", 0 }
            };
            
            // Get task completion trend by week (last 8 weeks)
            var startDate = DateTime.Now.AddDays(-56); // 8 weeks ago
            var weeklyCompletionTrend = new List<object>();
            
            // Get tasks per user
            var tasksPerUser = new Dictionary<string, int>();
            
            // Lấy tất cả các AssignedToId từ các task
            var allAssignedUserIds = projects
                .SelectMany(p => p.Tasks)
                .Where(t => !string.IsNullOrEmpty(t.AssignedToId))
                .Select(t => t.AssignedToId)
                .Distinct()
                .ToList();
                
            // Truy vấn tất cả users trong một lần
            var usersDict = new Dictionary<string, ApplicationUser>();
            foreach (var userId in allAssignedUserIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    usersDict[userId] = user;
                }
            }
            
            // Process all tasks from user's projects
            foreach (var project in projects)
            {
                foreach (var task in project.Tasks)
                {
                    // Count by status
                    switch (task.Status)
                    {
                        case ProjectTaskStatus.NotStarted:
                            tasksByStatus["Not Started"]++;
                            break;
                        case ProjectTaskStatus.InProgress:
                            tasksByStatus["In Progress"]++;
                            break;
                        case ProjectTaskStatus.Completed:
                            tasksByStatus["Completed"]++;
                            break;
                        case ProjectTaskStatus.OnHold:
                            tasksByStatus["On Hold"]++;
                            break;
                    }
                    
                    // Count by priority
                    switch (task.Priority)
                    {
                        case Priority.Low:
                            tasksByPriority["Low"]++;
                            break;
                        case Priority.Medium:
                            tasksByPriority["Medium"]++;
                            break;
                        case Priority.High:
                            tasksByPriority["High"]++;
                            break;
                    }
                    
                    // Count tasks per user
                    if (!string.IsNullOrEmpty(task.AssignedToId))
                    {
                        // Sử dụng dictionary để lấy thông tin user
                        string userName = "Unknown User";
                        if (usersDict.TryGetValue(task.AssignedToId, out var assignedUser))
                        {
                            userName = $"{assignedUser.FirstName} {assignedUser.LastName}";
                        }
                            
                        if (!tasksPerUser.ContainsKey(userName))
                        {
                            tasksPerUser[userName] = 0;
                        }
                        
                        tasksPerUser[userName]++;
                    }
                }
            }
            
            // Get weekly completion data - FIXED by first fetching the data and then doing grouping client-side
            var completedTasksQuery = await _context.ProjectTasks
                .Where(t => t.Status == ProjectTaskStatus.Completed && 
                           t.CreatedAt >= startDate &&
                           (isAdmin || t.Project.CreatedById == currentUser.Id || 
                            t.Project.ProjectMembers.Any(pm => pm.UserId == currentUser.Id)))
                .ToListAsync();
                
            // Perform the grouping on the client side
            var weeklyCompletedTasks = completedTasksQuery
                .GroupBy(t => new { 
                    Year = t.CreatedAt.Year, 
                    Week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                        t.CreatedAt, 
                        CalendarWeekRule.FirstDay, 
                        DayOfWeek.Sunday)
                })
                .Select(g => new { 
                    YearWeek = $"{g.Key.Year}-W{g.Key.Week}", 
                    Count = g.Count() 
                })
                .OrderBy(x => x.YearWeek)
                .ToList();
                
            // Fill in the weekly trend data
            for (int i = 0; i < 8; i++)
            {
                var date = DateTime.Now.AddDays(-7 * (7 - i));
                var year = date.Year;
                var week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    date, 
                    CalendarWeekRule.FirstDay, 
                    DayOfWeek.Sunday);
                    
                var yearWeek = $"{year}-W{week}";
                var count = weeklyCompletedTasks.FirstOrDefault(w => w.YearWeek == yearWeek)?.Count ?? 0;
                
                weeklyCompletionTrend.Add(new { 
                    week = $"Week {week}", 
                    count = count 
                });
            }
            
            // Monthly completion trend (last 6 months) - FIXED similarly
            var monthlyCompletedTasksQuery = await _context.ProjectTasks
                .Where(t => t.Status == ProjectTaskStatus.Completed && 
                           t.CreatedAt >= DateTime.Now.AddMonths(-6) &&
                           (isAdmin || t.Project.CreatedById == currentUser.Id || 
                            t.Project.ProjectMembers.Any(pm => pm.UserId == currentUser.Id)))
                .ToListAsync();
                
            var monthlyCompletionTrend = monthlyCompletedTasksQuery
                .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
                .Select(g => new { 
                    Month = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month)} {g.Key.Year}", 
                    Count = g.Count() 
                })
                .OrderBy(x => x.Month)
                .ToList();
            
            var model = new ReportViewModel
            {
                TasksByStatus = tasksByStatus,
                TasksByPriority = tasksByPriority,
                TasksPerUser = tasksPerUser,
                WeeklyCompletionTrend = weeklyCompletionTrend,
                MonthlyCompletionTrend = monthlyCompletionTrend
            };
            
            return View(model);
        }
        
        // GET: Report/ProjectReport/5
        public async Task<IActionResult> ProjectReport(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Tasks)
                .ThenInclude(t => t.AssignedTo)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (project == null)
            {
                return NotFound();
            }
            
            // Check if user has access to this project
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var isMember = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == id && pm.UserId == currentUser.Id);
                
            if (!isAdmin && !isMember)
            {
                return Forbid();
            }
            
            // Task statistics by status
            var tasksByStatus = new Dictionary<string, int>
            {
                { "Not Started", project.Tasks.Count(t => t.Status == ProjectTaskStatus.NotStarted) },
                { "In Progress", project.Tasks.Count(t => t.Status == ProjectTaskStatus.InProgress) },
                { "Completed", project.Tasks.Count(t => t.Status == ProjectTaskStatus.Completed) },
                { "On Hold", project.Tasks.Count(t => t.Status == ProjectTaskStatus.OnHold) }
            };
            
            // Task statistics by priority
            var tasksByPriority = new Dictionary<string, int>
            {
                { "Low", project.Tasks.Count(t => t.Priority == Priority.Low) },
                { "Medium", project.Tasks.Count(t => t.Priority == Priority.Medium) },
                { "High", project.Tasks.Count(t => t.Priority == Priority.High) }
            };
            
            // Tasks per user
            var tasksPerUser = project.Tasks
                .Where(t => t.AssignedTo != null)
                .GroupBy(t => $"{t.AssignedTo.FirstName} {t.AssignedTo.LastName}")
                .ToDictionary(g => g.Key, g => g.Count());
                
            // Weekly completion trend
            var startDate = DateTime.Now.AddDays(-56); // 8 weeks ago
            var weeklyCompletionTrend = new List<object>();
            
            // Group tasks by week - already client-side since we have project.Tasks in memory
            var completedTasksByWeek = project.Tasks
                .Where(t => t.Status == ProjectTaskStatus.Completed && t.CreatedAt >= startDate)
                .GroupBy(t => new { 
                    Year = t.CreatedAt.Year, 
                    Week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                        t.CreatedAt, 
                        CalendarWeekRule.FirstDay, 
                        DayOfWeek.Sunday)
                })
                .ToDictionary(
                    g => $"{g.Key.Year}-W{g.Key.Week}", 
                    g => g.Count()
                );
                
            // Fill in the weekly trend data
            for (int i = 0; i < 8; i++)
            {
                var date = DateTime.Now.AddDays(-7 * (7 - i));
                var year = date.Year;
                var week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    date, 
                    CalendarWeekRule.FirstDay, 
                    DayOfWeek.Sunday);
                    
                var yearWeek = $"{year}-W{week}";
                var count = completedTasksByWeek.ContainsKey(yearWeek) ? completedTasksByWeek[yearWeek] : 0;
                
                weeklyCompletionTrend.Add(new { 
                    week = $"Week {week}", 
                    count = count 
                });
            }
            
            var model = new ProjectReportViewModel
            {
                Project = project,
                TasksByStatus = tasksByStatus,
                TasksByPriority = tasksByPriority,
                TasksPerUser = tasksPerUser,
                WeeklyCompletionTrend = weeklyCompletionTrend
            };
            
            return View(model);
        }
        
        // GET: Report/UserReport
        [Authorize]
        public async Task<IActionResult> UserReport(string? userId = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            
            // If userId is not provided or user is not admin, show current user's report
            if (string.IsNullOrEmpty(userId) || !isAdmin)
            {
                userId = currentUser.Id;
            }
            
            // Get user details
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            
            // Get tasks assigned to user
            var tasks = await _context.ProjectTasks
                .Include(t => t.Project)
                .Where(t => t.AssignedToId == userId)
                .ToListAsync();
                
            // Task statistics by status
            var tasksByStatus = new Dictionary<string, int>
            {
                { "Not Started", tasks.Count(t => t.Status == ProjectTaskStatus.NotStarted) },
                { "In Progress", tasks.Count(t => t.Status == ProjectTaskStatus.InProgress) },
                { "Completed", tasks.Count(t => t.Status == ProjectTaskStatus.Completed) },
                { "On Hold", tasks.Count(t => t.Status == ProjectTaskStatus.OnHold) }
            };
            
            // Task statistics by priority
            var tasksByPriority = new Dictionary<string, int>
            {
                { "Low", tasks.Count(t => t.Priority == Priority.Low) },
                { "Medium", tasks.Count(t => t.Priority == Priority.Medium) },
                { "High", tasks.Count(t => t.Priority == Priority.High) }
            };
            
            // Tasks by project
            var tasksByProject = tasks
                .GroupBy(t => t.Project.Name)
                .ToDictionary(g => g.Key, g => g.Count());
                
            // Weekly completion trend
            var startDate = DateTime.Now.AddDays(-56); // 8 weeks ago
            var weeklyCompletionTrend = new List<object>();
            
            // Group tasks by week - already client-side since we have tasks in memory
            var completedTasksByWeek = tasks
                .Where(t => t.Status == ProjectTaskStatus.Completed && t.CreatedAt >= startDate)
                .GroupBy(t => new { 
                    Year = t.CreatedAt.Year, 
                    Week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                        t.CreatedAt, 
                        CalendarWeekRule.FirstDay, 
                        DayOfWeek.Sunday)
                })
                .ToDictionary(
                    g => $"{g.Key.Year}-W{g.Key.Week}", 
                    g => g.Count()
                );
                
            // Fill in the weekly trend data
            for (int i = 0; i < 8; i++)
            {
                var date = DateTime.Now.AddDays(-7 * (7 - i));
                var year = date.Year;
                var week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    date, 
                    CalendarWeekRule.FirstDay, 
                    DayOfWeek.Sunday);
                    
                var yearWeek = $"{year}-W{week}";
                var count = completedTasksByWeek.ContainsKey(yearWeek) ? completedTasksByWeek[yearWeek] : 0;
                
                weeklyCompletionTrend.Add(new { 
                    week = $"Week {week}", 
                    count = count 
                });
            }
            
            // Get all users for admin dropdown
            var users = new List<ApplicationUser>();
            if (isAdmin)
            {
                users = await _userManager.Users.ToListAsync();
            }
            
            var model = new UserReportViewModel
            {
                User = user,
                Tasks = tasks,
                TasksByStatus = tasksByStatus,
                TasksByPriority = tasksByPriority,
                TasksByProject = tasksByProject,
                WeeklyCompletionTrend = weeklyCompletionTrend,
                Users = users,
                IsAdmin = isAdmin
            };
            
            return View(model);
        }
        
        // GET: Report/GetTasksData
        [HttpGet]
        public async Task<IActionResult> GetTasksData()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            
            // Get projects the user has access to
            var projectsQuery = _context.Projects
                .Include(p => p.Tasks)
                .AsQueryable();

            if (!isAdmin)
            {
                projectsQuery = projectsQuery.Where(p => 
                    p.CreatedById == currentUser.Id || 
                    p.ProjectMembers.Any(pm => pm.UserId == currentUser.Id));
            }

            var projects = await projectsQuery.ToListAsync();
            
            // Task statistics by status - done client-side after loading the data
            var tasksByStatus = new[]
            {
                new { 
                    status = "Not Started", 
                    count = projects.SelectMany(p => p.Tasks).Count(t => t.Status == ProjectTaskStatus.NotStarted)
                },
                new { 
                    status = "In Progress", 
                    count = projects.SelectMany(p => p.Tasks).Count(t => t.Status == ProjectTaskStatus.InProgress)
                },
                new { 
                    status = "Completed", 
                    count = projects.SelectMany(p => p.Tasks).Count(t => t.Status == ProjectTaskStatus.Completed)
                },
                new { 
                    status = "On Hold", 
                    count = projects.SelectMany(p => p.Tasks).Count(t => t.Status == ProjectTaskStatus.OnHold)
                }
            };
            
            return Json(tasksByStatus);
        }
    }
} 