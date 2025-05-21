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
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Lấy tất cả bình luận của một task
        /// </summary>
        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetTaskComments(int taskId)
        {
            var task = await _context.ProjectTasks.FindAsync(taskId);
            if (task == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Công việc không tồn tại", HttpStatusCode.NotFound));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Kiểm tra quyền truy cập
            var projectTask = await _context.ProjectTasks
                .Include(t => t.Project)
                    .ThenInclude(p => p.ProjectMembers)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (projectTask == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Công việc không tồn tại", HttpStatusCode.NotFound));
            }

            if (!isAdmin && 
                projectTask.Project.CreatedById != userId && 
                !projectTask.Project.ProjectMembers.Any(pm => pm.UserId == userId) &&
                projectTask.CreatedById != userId &&
                projectTask.AssignedToId != userId)
            {
                return Forbid();
            }

            var comments = await _context.TaskComments
                .Include(c => c.User)
                .Include(c => c.Mentions)
                    .ThenInclude(m => m.MentionedUser)
                .Where(c => c.TaskId == taskId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return Ok(ApiResponse.SuccessResponse("Lấy danh sách bình luận thành công", comments));
        }

        /// <summary>
        /// Tạo bình luận mới cho task
        /// </summary>
        [HttpPost("task/{taskId}")]
        public async Task<IActionResult> CreateComment(int taskId, [FromBody] TaskComment comment)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                    .ThenInclude(p => p.ProjectMembers)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Công việc không tồn tại", HttpStatusCode.NotFound));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Kiểm tra quyền thêm bình luận
            if (!isAdmin && 
                task.Project.CreatedById != userId && 
                !task.Project.ProjectMembers.Any(pm => pm.UserId == userId) &&
                task.CreatedById != userId &&
                task.AssignedToId != userId)
            {
                return Forbid();
            }

            comment.TaskId = taskId;
            comment.UserId = userId;
            comment.CreatedAt = DateTime.Now;

            _context.TaskComments.Add(comment);
            await _context.SaveChangesAsync();

            // Tải lại comment cùng với thông tin user
            var savedComment = await _context.TaskComments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            return CreatedAtAction(nameof(GetTaskComments), new { taskId = taskId }, 
                ApiResponse.SuccessResponse("Thêm bình luận thành công", savedComment));
        }

        /// <summary>
        /// Xóa bình luận
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.TaskComments
                .Include(c => c.Task)
                    .ThenInclude(t => t.Project)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Bình luận không tồn tại", HttpStatusCode.NotFound));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Chỉ người tạo bình luận, admin hoặc người tạo task/project mới có quyền xóa
            if (!isAdmin && 
                comment.UserId != userId && 
                comment.Task.CreatedById != userId && 
                comment.Task.Project.CreatedById != userId)
            {
                return Forbid();
            }

            _context.TaskComments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.SuccessResponse("Xóa bình luận thành công"));
        }

        /// <summary>
        /// Kiểm tra API controller hoạt động
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Comments controller is working");
        }
    }
} 