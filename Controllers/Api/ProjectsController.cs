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
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Lấy danh sách dự án
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var query = _context.Projects.AsQueryable();

            // Nếu không phải admin, chỉ lấy các project mà user là creator hoặc member
            if (!isAdmin)
            {
                query = query.Where(p => 
                    p.CreatedById == userId || 
                    p.ProjectMembers.Any(pm => pm.UserId == userId));
            }

            var projects = await query
                .Include(p => p.CreatedBy)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(ApiResponse.SuccessResponse("Lấy danh sách dự án thành công", projects));
        }

        /// <summary>
        /// Lấy thông tin chi tiết dự án
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var project = await _context.Projects
                .Include(p => p.CreatedBy)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.User)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.AssignedTo)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Dự án không tồn tại", HttpStatusCode.NotFound));
            }

            // Kiểm tra quyền truy cập
            if (!isAdmin && project.CreatedById != userId && !project.ProjectMembers.Any(pm => pm.UserId == userId))
            {
                return Forbid();
            }

            return Ok(ApiResponse.SuccessResponse("Lấy thông tin dự án thành công", project));
        }

        /// <summary>
        /// Tạo dự án mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] Project project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Dữ liệu không hợp lệ", HttpStatusCode.BadRequest, ModelState));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            project.CreatedById = userId;
            project.CreatedAt = DateTime.Now;

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, 
                ApiResponse.SuccessResponse("Tạo dự án thành công", project));
        }

        /// <summary>
        /// Cập nhật dự án
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] Project project)
        {
            if (id != project.Id)
            {
                return BadRequest(ApiResponse.ErrorResponse("ID không khớp", HttpStatusCode.BadRequest));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Dữ liệu không hợp lệ", HttpStatusCode.BadRequest, ModelState));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var existingProject = await _context.Projects.FindAsync(id);
            if (existingProject == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Dự án không tồn tại", HttpStatusCode.NotFound));
            }

            // Kiểm tra quyền chỉnh sửa
            if (!isAdmin && existingProject.CreatedById != userId)
            {
                return Forbid();
            }

            existingProject.Name = project.Name;
            existingProject.Description = project.Description;
            existingProject.StartDate = project.StartDate;
            existingProject.EndDate = project.EndDate;
            existingProject.Status = project.Status;
            existingProject.UpdatedAt = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
                {
                    return NotFound(ApiResponse.ErrorResponse("Dự án không tồn tại", HttpStatusCode.NotFound));
                }
                else
                {
                    throw;
                }
            }

            return Ok(ApiResponse.SuccessResponse("Cập nhật dự án thành công", existingProject));
        }

        /// <summary>
        /// Xóa dự án
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Dự án không tồn tại", HttpStatusCode.NotFound));
            }

            // Kiểm tra quyền xóa
            if (!isAdmin && project.CreatedById != userId)
            {
                return Forbid();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.SuccessResponse("Xóa dự án thành công"));
        }

        /// <summary>
        /// Thêm thành viên vào dự án
        /// </summary>
        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddMember(int id, [FromBody] ProjectMember member)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Dự án không tồn tại", HttpStatusCode.NotFound));
            }

            // Kiểm tra quyền thêm thành viên
            if (!isAdmin && project.CreatedById != userId)
            {
                return Forbid();
            }

            // Kiểm tra user có tồn tại không
            var user = await _userManager.FindByIdAsync(member.UserId);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Người dùng không tồn tại", HttpStatusCode.NotFound));
            }

            // Kiểm tra user có trong dự án chưa
            if (project.ProjectMembers.Any(pm => pm.UserId == member.UserId))
            {
                return BadRequest(ApiResponse.ErrorResponse("Người dùng đã là thành viên của dự án", HttpStatusCode.BadRequest));
            }

            member.ProjectId = id;
            member.JoinedAt = DateTime.Now;

            _context.ProjectMembers.Add(member);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.SuccessResponse("Thêm thành viên thành công", member));
        }

        /// <summary>
        /// Xóa thành viên khỏi dự án
        /// </summary>
        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(int id, string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Dự án không tồn tại", HttpStatusCode.NotFound));
            }

            // Kiểm tra quyền xóa thành viên
            if (!isAdmin && project.CreatedById != currentUserId && currentUserId != userId)
            {
                return Forbid();
            }

            var member = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == userId);
            if (member == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Người dùng không phải là thành viên của dự án", HttpStatusCode.NotFound));
            }

            _context.ProjectMembers.Remove(member);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.SuccessResponse("Xóa thành viên thành công"));
        }

        // Thêm một endpoint test không yêu cầu xác thực
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Projects controller is working");
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
} 