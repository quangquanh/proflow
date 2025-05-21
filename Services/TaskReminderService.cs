using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Services
{
    public class TaskReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TaskReminderService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

        public TaskReminderService(
            IServiceProvider serviceProvider,
            ILogger<TaskReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Task Reminder Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Task Reminder Service is checking for upcoming deadlines.");

                try
                {
                    await CheckUpcomingDeadlines(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while checking for upcoming deadlines.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Task Reminder Service is stopping.");
        }

        private async Task CheckUpcomingDeadlines(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Get all tasks that are not completed, deadline is within 24 hours, 
            // and reminder hasn't been sent yet
            var now = DateTime.Now;
            var oneDayAhead = now.AddHours(24);

            var tasksToRemind = await dbContext.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .Where(t => t.Status != ProjectTaskStatus.Completed 
                         && t.Deadline > now
                         && t.Deadline <= oneDayAhead
                         && !t.ReminderSent
                         && t.AssignedToId != null)
                .ToListAsync(cancellationToken);

            foreach (var task in tasksToRemind)
            {
                // In a real implementation, you would send an email or push notification here
                _logger.LogInformation("Sending reminder for task {TaskId}: {TaskTitle} to {UserEmail}. " +
                                       "Deadline: {Deadline}",
                                      task.Id,
                                      task.Title,
                                      task.AssignedTo.Email,
                                      task.Deadline.ToString("g"));

                // For now, just mark the reminder as sent
                task.ReminderSent = true;
            }

            if (tasksToRemind.Any())
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Sent reminders for {Count} tasks", tasksToRemind.Count);
            }
        }
    }
} 