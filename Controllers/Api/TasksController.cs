using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Models.API;
using System.Net;
using System.Security.Claims;

namespace ProjectManagementSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    // Tạm thời comment out để test
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Lấy danh sách task, có thể lọc theo project
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTasks([FromQuery] int? projectId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var query = _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .AsQueryable();

            // Lọc theo project nếu có
            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }

            // Nếu không phải admin, chỉ lấy các task mà user có quyền xem
            if (!isAdmin)
            {
                query = query.Where(t => 
                    t.Project.CreatedById == userId || 
                    t.Project.ProjectMembers.Any(pm => pm.UserId == userId) ||
                    t.AssignedToId == userId ||
                    t.CreatedById == userId);
            }

            var tasks = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(ApiResponse.SuccessResponse("Lấy danh sách công việc thành công", tasks));
        }

        /// <summary>
        /// Lấy thông tin chi tiết task
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Công việc không tồn tại", HttpStatusCode.NotFound));
            }

            // Kiểm tra quyền truy cập
            if (!isAdmin && 
                task.Project.CreatedById != userId && 
                !task.Project.ProjectMembers.Any(pm => pm.UserId == userId) &&
                task.AssignedToId != userId &&
                task.CreatedById != userId)
            {
                return Forbid();
            }

            return Ok(ApiResponse.SuccessResponse("Lấy thông tin công việc thành công", task));
        }

        /// <summary>
        /// Tạo task mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] ProjectTask task)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Dữ liệu không hợp lệ", HttpStatusCode.BadRequest, ModelState));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Kiểm tra project có tồn tại không
            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.Id == task.ProjectId);

            if (project == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Dự án không tồn tại", HttpStatusCode.NotFound));
            }

            // Kiểm tra quyền tạo task
            if (!isAdmin && 
                project.CreatedById != userId && 
                !project.ProjectMembers.Any(pm => pm.UserId == userId))
            {
                return Forbid();
            }

            // Kiểm tra người được gán task có tồn tại không
            if (!string.IsNullOrEmpty(task.AssignedToId))
            {
                var assignedUser = await _userManager.FindByIdAsync(task.AssignedToId);
                if (assignedUser == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("Người dùng được gán không tồn tại", HttpStatusCode.NotFound));
                }

                // Kiểm tra người được gán có trong dự án không
                if (project.CreatedById != task.AssignedToId && 
                    !project.ProjectMembers.Any(pm => pm.UserId == task.AssignedToId))
                {
                    return BadRequest(ApiResponse.ErrorResponse("Người dùng được gán không phải là thành viên của dự án", HttpStatusCode.BadRequest));
                }
            }

            task.CreatedById = userId;
            task.CreatedAt = DateTime.Now;

            _context.ProjectTasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, 
                ApiResponse.SuccessResponse("Tạo công việc thành công", task));
        }

        /// <summary>
        /// Cập nhật task
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] ProjectTask task)
        {
            if (id != task.Id)
            {
                return BadRequest(ApiResponse.ErrorResponse("ID không khớp", HttpStatusCode.BadRequest));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Dữ liệu không hợp lệ", HttpStatusCode.BadRequest, ModelState));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var existingTask = await _context.ProjectTasks
                .Include(t => t.Project)
                    .ThenInclude(p => p.ProjectMembers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existingTask == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Công việc không tồn tại", HttpStatusCode.NotFound));
            }

            // Kiểm tra quyền chỉnh sửa
            if (!isAdmin && 
                existingTask.Project.CreatedById != userId && 
                existingTask.CreatedById != userId &&
                existingTask.AssignedToId != userId)
            {
                return Forbid();
            }

            // Kiểm tra người được gán task có tồn tại không
            if (!string.IsNullOrEmpty(task.AssignedToId))
            {
                var assignedUser = await _userManager.FindByIdAsync(task.AssignedToId);
                if (assignedUser == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("Người dùng được gán không tồn tại", HttpStatusCode.NotFound));
                }

                // Kiểm tra người được gán có trong dự án không
                if (existingTask.Project.CreatedById != task.AssignedToId && 
                    !existingTask.Project.ProjectMembers.Any(pm => pm.UserId == task.AssignedToId))
                {
                    return BadRequest(ApiResponse.ErrorResponse("Người dùng được gán không phải là thành viên của dự án", HttpStatusCode.BadRequest));
                }
            }

            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.Status = task.Status;
            existingTask.Priority = task.Priority;
            existingTask.Deadline = task.Deadline;
            existingTask.AssignedToId = task.AssignedToId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    return NotFound(ApiResponse.ErrorResponse("Công việc không tồn tại", HttpStatusCode.NotFound));
                }
                else
                {
                    throw;
                }
            }

            return Ok(ApiResponse.SuccessResponse("Cập nhật công việc thành công", existingTask));
        }

        /// <summary>
        /// Xóa task
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Công việc không tồn tại", HttpStatusCode.NotFound));
            }

            // Kiểm tra quyền xóa
            if (!isAdmin && task.Project.CreatedById != userId && task.CreatedById != userId)
            {
                return Forbid();
            }

            _context.ProjectTasks.Remove(task);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.SuccessResponse("Xóa công việc thành công"));
        }

        /// <summary>
        /// Lấy danh sách task của người dùng hiện tại
        /// </summary>
        [HttpGet("myTasks")]
        public async Task<IActionResult> GetMyTasks()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var tasks = await _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Where(t => t.AssignedToId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(ApiResponse.SuccessResponse("Lấy danh sách công việc của người dùng thành công", tasks));
        }

        // Thêm một endpoint test không yêu cầu xác thực
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Tasks controller is working");
        }

        private bool TaskExists(int id)
        {
            return _context.ProjectTasks.Any(e => e.Id == id);
        }
    }
} 