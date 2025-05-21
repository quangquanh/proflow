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
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Lấy danh sách người dùng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Chỉ admin mới có quyền xem danh sách tất cả người dùng
            if (!isAdmin)
            {
                return Forbid();
            }

            var users = await _userManager.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.LastName)
                .ToListAsync();

            return Ok(ApiResponse.SuccessResponse("Lấy danh sách người dùng thành công", users));
        }

        /// <summary>
        /// Lấy thông tin chi tiết người dùng
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Kiểm tra quyền xem thông tin người dùng
            if (!isAdmin && id != currentUserId)
            {
                return Forbid();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Người dùng không tồn tại", HttpStatusCode.NotFound));
            }

            return Ok(ApiResponse.SuccessResponse("Lấy thông tin người dùng thành công", user));
        }

        /// <summary>
        /// Lấy thông tin người dùng hiện tại
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Người dùng không tồn tại", HttpStatusCode.NotFound));
            }

            // Lấy thêm các dự án và đội nhóm của người dùng
            var projects = await _context.ProjectMembers
                .Include(pm => pm.Project)
                .Where(pm => pm.UserId == userId)
                .Select(pm => pm.Project)
                .ToListAsync();

            var teams = await _context.TeamMembers
                .Include(tm => tm.Team)
                .Where(tm => tm.UserId == userId)
                .Select(tm => tm.Team)
                .ToListAsync();

            var assignedTasks = await _context.ProjectTasks
                .Include(t => t.Project)
                .Where(t => t.AssignedToId == userId)
                .ToListAsync();

            var result = new
            {
                User = user,
                Projects = projects,
                Teams = teams,
                AssignedTasks = assignedTasks
            };

            return Ok(ApiResponse.SuccessResponse("Lấy thông tin người dùng hiện tại thành công", result));
        }

        /// <summary>
        /// Cập nhật thông tin người dùng
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] ApplicationUser userUpdate)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Kiểm tra quyền cập nhật thông tin người dùng
            if (!isAdmin && id != currentUserId)
            {
                return Forbid();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Người dùng không tồn tại", HttpStatusCode.NotFound));
            }

            // Chỉ cho phép cập nhật một số thông tin cá nhân
            user.FirstName = userUpdate.FirstName;
            user.LastName = userUpdate.LastName;
            user.PhoneNumber = userUpdate.PhoneNumber;
            
            // Chỉ admin mới có quyền cập nhật trạng thái
            if (isAdmin)
            {
                user.IsActive = userUpdate.IsActive;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    ApiResponse.ErrorResponse("Lỗi khi cập nhật người dùng", HttpStatusCode.InternalServerError, errors));
            }

            return Ok(ApiResponse.SuccessResponse("Cập nhật thông tin người dùng thành công", user));
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Dữ liệu không hợp lệ", HttpStatusCode.BadRequest, ModelState));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Người dùng không tồn tại", HttpStatusCode.NotFound));
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    ApiResponse.ErrorResponse("Lỗi khi đổi mật khẩu", HttpStatusCode.InternalServerError, errors));
            }

            return Ok(ApiResponse.SuccessResponse("Đổi mật khẩu thành công"));
        }

        /// <summary>
        /// Kiểm tra API controller hoạt động
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Users controller is working");
        }
    }

    public class ChangePasswordModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
} 