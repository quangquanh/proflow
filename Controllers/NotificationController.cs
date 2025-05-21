using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Hubs;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Services;
using ProjectManagementSystem.ViewModels;

namespace ProjectManagementSystem.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        // GET: Notification/GetUserNotifications
        [HttpGet]
        public async Task<IActionResult> GetUserNotifications()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var notifications = await _context.Notifications
                .Where(n => n.UserId == currentUser.Id)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20) // Giới hạn 20 thông báo mới nhất
                .Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Content = n.Content,
                    CreatedAt = n.CreatedAt,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    Link = n.Link
                })
                .ToListAsync();

            return PartialView("_Notifications", notifications);
        }

        // GET: Notification/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var notifications = await _context.Notifications
                .Where(n => n.UserId == currentUser.Id)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Content = n.Content,
                    CreatedAt = n.CreatedAt,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    Link = n.Link
                })
                .ToListAsync();

            return View(notifications);
        }

        // GET: Notification/GetUnreadCount
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var unreadCount = await _context.Notifications
                .Where(n => n.UserId == currentUser.Id && !n.IsRead)
                .CountAsync();

            return Json(new { count = unreadCount });
        }

        // POST: Notification/MarkAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == currentUser.Id);

            if (notification == null)
            {
                return NotFound();
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: Notification/MarkAllAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == currentUser.Id && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        // POST: Notification/SendTestNotification
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTestNotification(string userId, string message, string recipientType)
        {
            if (string.IsNullOrEmpty(message))
            {
                return BadRequest("Message is required");
            }

            if (recipientType == "specific" && string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required when sending to specific user");
            }

            try
            {
                if (recipientType == "all")
                {
                    // Get all users
                    var users = await _userManager.Users.ToListAsync();
                    var notifications = new List<Notification>();

                    // Create notifications for all users
                    foreach (var user in users)
                    {
                        var notification = new Notification
                        {
                            Content = message,
                            CreatedAt = DateTime.Now,
                            UserId = user.Id,
                            Type = NotificationType.Mention,
                            IsRead = false,
                            Link = Url.Action("Index", "Home")
                        };
                        notifications.Add(notification);
                    }

                    // Add all notifications to database
                    await _context.Notifications.AddRangeAsync(notifications);
                    await _context.SaveChangesAsync();

                    // Send real-time notifications to all users
                    foreach (var notification in notifications)
                    {
                        await NotificationService.SendRealTimeNotification(_hubContext, notification);
                    }

                    return Json(new { success = true, message = $"Test notification sent to {users.Count} users" });
                }
                else
                {
                    // Send to specific user
                    var notification = new Notification
                    {
                        Content = message,
                        CreatedAt = DateTime.Now,
                        UserId = userId,
                        Type = NotificationType.Mention,
                        IsRead = false,
                        Link = Url.Action("Index", "Home")
                    };

                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();

                    // Send real-time notification
                    await NotificationService.SendRealTimeNotification(_hubContext, notification);

                    return Json(new { success = true, message = "Test notification sent to specific user" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error sending notification: {ex.Message}");
            }
        }
    }
} 