using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Hubs;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Services
{
    public class NotificationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IServiceScopeFactory scopeFactory,
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationService> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Service is running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Check for task deadlines approaching
                    await CheckTaskDeadlines();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Notification Service");
                }

                // Run every hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task CheckTaskDeadlines()
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Find tasks with deadlines within the next 24 hours that don't already have a notification
            var tomorrow = DateTime.Now.AddDays(1);
            var tasksWithApproachingDeadlines = await dbContext.ProjectTasks
                .Include(t => t.AssignedTo)
                .Where(t => t.Deadline.Date == tomorrow.Date && 
                            t.Status != ProjectTaskStatus.Completed &&
                            !dbContext.Notifications.Any(n => 
                                n.TaskId == t.Id && 
                                n.Type == NotificationType.TaskDue &&
                                n.CreatedAt.Date == DateTime.Now.Date))
                .ToListAsync();

            foreach (var task in tasksWithApproachingDeadlines)
            {
                if (!string.IsNullOrEmpty(task.AssignedToId))
                {
                    // Create notification in database
                    var notification = new Notification
                    {
                        Content = $"Task '{task.Title}' is due tomorrow!",
                        CreatedAt = DateTime.Now,
                        UserId = task.AssignedToId,
                        Type = NotificationType.TaskDue,
                        TaskId = task.Id,
                        IsRead = false,
                        Link = $"/Task/Details/{task.Id}"
                    };

                    dbContext.Notifications.Add(notification);
                    await dbContext.SaveChangesAsync();

                    // Send real-time notification
                    await _hubContext.Clients.User(task.AssignedToId)
                        .SendAsync("ReceiveNotification", new
                        {
                            id = notification.Id,
                            content = notification.Content,
                            createdAt = notification.CreatedAt,
                            type = notification.Type.ToString(),
                            isRead = notification.IsRead,
                            link = notification.Link
                        });
                }
            }
        }
        
        // Method to be called from controllers to send real-time notifications
        public static async Task SendRealTimeNotification(
            IHubContext<NotificationHub> hubContext, 
            Notification notification)
        {
            if (notification != null && !string.IsNullOrEmpty(notification.UserId))
            {
                await hubContext.Clients.User(notification.UserId)
                    .SendAsync("ReceiveNotification", new
                    {
                        id = notification.Id,
                        content = notification.Content,
                        createdAt = notification.CreatedAt,
                        type = notification.Type.ToString(),
                        isRead = notification.IsRead,
                        link = notification.Link
                    });
            }
        }
    }
} 