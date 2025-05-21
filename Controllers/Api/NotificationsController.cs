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
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Lấy tất cả thông báo của người dùng hiện tại
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var query = _context.Notifications
                .Where(n => n.UserId == userId);
                
            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }
            
            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
                
            return Ok(ApiResponse.SuccessResponse("Lấy danh sách thông báo thành công", notifications));
        }

        /// <summary>
        /// Đánh dấu thông báo đã đọc
        /// </summary>
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
                
            if (notification == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Thông báo không tồn tại", HttpStatusCode.NotFound));
            }
            
            notification.IsRead = true;
            await _context.SaveChangesAsync();
            
            return Ok(ApiResponse.SuccessResponse("Đã đánh dấu thông báo là đã đọc"));
        }

        /// <summary>
        /// Đánh dấu tất cả thông báo là đã đọc
        /// </summary>
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
                
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }
            
            await _context.SaveChangesAsync();
            
            return Ok(ApiResponse.SuccessResponse($"Đã đánh dấu {unreadNotifications.Count} thông báo là đã đọc"));
        }

        /// <summary>
        /// Xóa một thông báo
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
                
            if (notification == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Thông báo không tồn tại", HttpStatusCode.NotFound));
            }
            
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            
            return Ok(ApiResponse.SuccessResponse("Đã xóa thông báo"));
        }

        /// <summary>
        /// Lấy số lượng thông báo chưa đọc
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var count = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
                
            return Ok(ApiResponse.SuccessResponse("Lấy số thông báo chưa đọc thành công", count));
        }
        
        /// <summary>
        /// Kiểm tra API controller hoạt động
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Notifications controller is working");
        }
    }
} 