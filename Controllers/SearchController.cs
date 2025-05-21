using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.ViewModels;

namespace ProjectManagementSystem.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SearchController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Search
        public async Task<IActionResult> Index(TaskSearchViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            
            // Chuẩn bị danh sách cho dropdowns
            await PrepareDropdownLists(model, currentUser, isAdmin);
            
            // Lấy danh sách projects user có quyền truy cập
            var projectsQuery = _context.Projects.AsQueryable();
            if (!isAdmin)
            {
                projectsQuery = projectsQuery.Where(p => 
                    p.CreatedById == currentUser.Id || 
                    p.ProjectMembers.Any(pm => pm.UserId == currentUser.Id));
            }
            
            var accessibleProjectIds = await projectsQuery
                .Select(p => p.Id)
                .ToListAsync();
            
            // Query cơ bản cho tasks
            var tasksQuery = _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Where(t => accessibleProjectIds.Contains(t.ProjectId));
            
            // Áp dụng bộ lọc
            // 1. Tìm kiếm theo từ khóa
            if (!string.IsNullOrEmpty(model.SearchTerm))
            {
                model.SearchTerm = model.SearchTerm.ToLower().Trim();
                tasksQuery = tasksQuery.Where(t => 
                    t.Title.ToLower().Contains(model.SearchTerm) || 
                    t.Description.ToLower().Contains(model.SearchTerm));
            }
            
            // 2. Lọc theo project
            if (model.ProjectId.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.ProjectId == model.ProjectId);
            }
            
            // 3. Lọc theo người được gán
            if (!string.IsNullOrEmpty(model.AssignedToId))
            {
                tasksQuery = tasksQuery.Where(t => t.AssignedToId == model.AssignedToId);
            }
            
            // 4. Lọc theo trạng thái
            if (model.Status.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.Status == model.Status);
            }
            
            // 5. Lọc theo ưu tiên
            if (model.Priority.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.Priority == model.Priority);
            }
            
            // 6. Lọc theo thời gian
            if (model.StartDate.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.CreatedAt >= model.StartDate.Value || t.Deadline >= model.StartDate.Value);
            }
            
            if (model.EndDate.HasValue)
            {
                // Thêm 1 ngày để bao gồm toàn bộ ngày cuối cùng
                var endDatePlusOne = model.EndDate.Value.AddDays(1);
                tasksQuery = tasksQuery.Where(t => t.CreatedAt < endDatePlusOne || t.Deadline < endDatePlusOne);
            }
            
            // Lấy tổng số tasks thỏa mãn điều kiện
            model.TotalTasks = await tasksQuery.CountAsync();
            
            // 7. Sắp xếp
            tasksQuery = ApplySorting(tasksQuery, model.SortBy, model.SortAscending);
            
            // 8. Phân trang
            var pageSize = model.PageSize;
            var skip = (model.Page - 1) * pageSize;
            
            model.Tasks = await tasksQuery
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
            
            return View(model);
        }
        
        private async Task PrepareDropdownLists(TaskSearchViewModel model, ApplicationUser currentUser, bool isAdmin)
        {
            // Lấy projects
            var projectsQuery = _context.Projects.AsQueryable();
            if (!isAdmin)
            {
                projectsQuery = projectsQuery.Where(p => 
                    p.CreatedById == currentUser.Id || 
                    p.ProjectMembers.Any(pm => pm.UserId == currentUser.Id));
            }
            
            model.AvailableProjects = await projectsQuery
                .OrderBy(p => p.Name)
                .ToListAsync();
            
            // Lấy danh sách người dùng
            if (isAdmin)
            {
                model.AvailableUsers = await _userManager.Users
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .ToListAsync();
            }
            else
            {
                // Nếu không phải admin, chỉ hiển thị người dùng trong các dự án của họ
                var projectIds = model.AvailableProjects.Select(p => p.Id);
                var userIdsInProjects = await _context.ProjectMembers
                    .Where(pm => projectIds.Contains(pm.ProjectId))
                    .Select(pm => pm.UserId)
                    .Distinct()
                    .ToListAsync();
                
                // Thêm người dùng hiện tại và người tạo dự án
                userIdsInProjects.Add(currentUser.Id);
                userIdsInProjects.AddRange(model.AvailableProjects.Select(p => p.CreatedById));
                
                model.AvailableUsers = await _userManager.Users
                    .Where(u => userIdsInProjects.Contains(u.Id))
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .ToListAsync();
            }
        }
        
        private IQueryable<ProjectTask> ApplySorting(IQueryable<ProjectTask> query, string sortBy, bool ascending)
        {
            return sortBy?.ToLower() switch
            {
                "title" => ascending 
                    ? query.OrderBy(t => t.Title) 
                    : query.OrderByDescending(t => t.Title),
                    
                "priority" => ascending 
                    ? query.OrderBy(t => t.Priority) 
                    : query.OrderByDescending(t => t.Priority),
                    
                "status" => ascending 
                    ? query.OrderBy(t => t.Status) 
                    : query.OrderByDescending(t => t.Status),
                    
                "deadline" => ascending 
                    ? query.OrderBy(t => t.Deadline) 
                    : query.OrderByDescending(t => t.Deadline),
                    
                "created" => ascending 
                    ? query.OrderBy(t => t.CreatedAt) 
                    : query.OrderByDescending(t => t.CreatedAt),
                    
                "project" => ascending 
                    ? query.OrderBy(t => t.Project.Name) 
                    : query.OrderByDescending(t => t.Project.Name),
                    
                "assignedto" => ascending 
                    ? query.OrderBy(t => t.AssignedTo.FirstName).ThenBy(t => t.AssignedTo.LastName) 
                    : query.OrderByDescending(t => t.AssignedTo.FirstName).ThenByDescending(t => t.AssignedTo.LastName),
                    
                _ => query.OrderByDescending(t => t.CreatedAt) // Mặc định sắp xếp theo CreatedAt giảm dần
            };
        }
    }
} 