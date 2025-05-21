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
using System.Text.RegularExpressions;

namespace ProjectManagementSystem.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _hubContext;

        public CommentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        // GET: Comment/GetComments/5 (taskId)
        [HttpGet]
        public async Task<IActionResult> GetComments(int taskId)
        {
            try
            {
                var task = await _context.ProjectTasks
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null)
                {
                    return NotFound();
                }

                // Check if user has access to the task
                var currentUser = await _userManager.GetUserAsync(User);
                var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
                var isMember = await _context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == currentUser.Id);
                var isCreator = task.CreatedById == currentUser.Id;
                var isAssigned = task.AssignedToId == currentUser.Id;

                if (!isAdmin && !isMember && !isCreator && !isAssigned)
                {
                    return Forbid();
                }

                // Get comments for the task
                var comments = await _context.TaskComments
                    .Include(c => c.User)
                    .Include(c => c.Mentions)
                    .ThenInclude(m => m.MentionedUser)
                    .Where(c => c.TaskId == taskId)
                    .OrderBy(c => c.CreatedAt)
                    .ToListAsync();

                // Lấy thông tin dự án chi tiết
                var project = await _context.Projects
                    .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.User)
                    .FirstOrDefaultAsync(p => p.Id == task.ProjectId);

                if (project == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Project with ID {task.ProjectId} not found!");
                    return NotFound("Project not found");
                }

                System.Diagnostics.Debug.WriteLine($"Project {project.Name} has {project.ProjectMembers.Count} members");

                // Get project members for mention suggestions
                var projectMembers = project.ProjectMembers
                    .Select(pm => new CommentUserViewModel
                    {
                        Id = pm.UserId,
                        FullName = $"{pm.User.FirstName} {pm.User.LastName}",
                        Username = pm.User.UserName,
                        ProfilePicture = pm.User.ProfilePicture ?? "/images/default-profile.png"
                    })
                    .ToList();

                // Log số lượng thành viên dự án tìm thấy để debug
                System.Diagnostics.Debug.WriteLine($"Found {projectMembers.Count} project members for project {task.ProjectId}");
                foreach (var member in projectMembers)
                {
                    System.Diagnostics.Debug.WriteLine($"Member: {member.FullName}, Username: {member.Username}");
                }

                var viewModel = new TaskCommentsViewModel
                {
                    TaskId = taskId,
                    TaskTitle = task.Title,
                    Comments = comments.Select(c => new CommentViewModel
                    {
                        Id = c.Id,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt,
                        UserId = c.UserId,
                        UserName = $"{c.User.FirstName} {c.User.LastName}",
                        UserProfilePicture = c.User.ProfilePicture ?? "/images/default-profile.png",
                        IsCurrentUserComment = c.UserId == currentUser.Id,
                        Mentions = c.Mentions.Select(m => new MentionViewModel
                        {
                            UserId = m.MentionedUserId,
                            UserName = $"{m.MentionedUser.FirstName} {m.MentionedUser.LastName}"
                        }).ToList()
                    }).ToList(),
                    NewComment = new CreateCommentViewModel { TaskId = taskId },
                    ProjectMembers = projectMembers
                };

                return PartialView("_Comments", viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetComments: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return Content($"Error loading comments: {ex.Message}");
            }
        }

        // POST: Comment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCommentViewModel model)
        {
            // Check if the model is null (can happen in some cases with Ajax)
            if (model == null)
            {
                ModelState.AddModelError("Content", "Comment content is required");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
            {
                // Get the task and associated data to rebuild the view model
                var taskItem = await _context.ProjectTasks
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(t => t.Id == model.TaskId);

                if (taskItem == null)
                {
                    return NotFound();
                }

                var currentUserInfo = await _userManager.GetUserAsync(User);
                
                // Get comments for the task
                var comments = await _context.TaskComments
                    .Include(c => c.User)
                    .Include(c => c.Mentions)
                    .ThenInclude(m => m.MentionedUser)
                    .Where(c => c.TaskId == model.TaskId)
                    .OrderBy(c => c.CreatedAt)
                    .ToListAsync();

                // Get project members for mention suggestions
                var projectMembers = await _context.ProjectMembers
                    .Include(pm => pm.User)
                    .Where(pm => pm.ProjectId == taskItem.ProjectId)
                    .Select(pm => new CommentUserViewModel
                    {
                        Id = pm.UserId,
                        FullName = $"{pm.User.FirstName} {pm.User.LastName}",
                        Username = pm.User.UserName,
                        ProfilePicture = pm.User.ProfilePicture ?? "/images/default-profile.png"
                    })
                    .ToListAsync();

                var viewModel = new TaskCommentsViewModel
                {
                    TaskId = model.TaskId,
                    TaskTitle = taskItem.Title,
                    Comments = comments.Select(c => new CommentViewModel
                    {
                        Id = c.Id,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt,
                        UserId = c.UserId,
                        UserName = $"{c.User.FirstName} {c.User.LastName}",
                        UserProfilePicture = c.User.ProfilePicture ?? "/images/default-profile.png",
                        IsCurrentUserComment = c.UserId == currentUserInfo.Id,
                        Mentions = c.Mentions.Select(m => new MentionViewModel
                        {
                            UserId = m.MentionedUserId,
                            UserName = $"{m.MentionedUser.FirstName} {m.MentionedUser.LastName}"
                        }).ToList()
                    }).ToList(),
                    NewComment = model, // Use the submitted model with validation errors
                    ProjectMembers = projectMembers
                };

                return PartialView("_Comments", viewModel);
            }

            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == model.TaskId);

            if (task == null)
            {
                return NotFound();
            }

            // Check if user has access to the task
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var isMember = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == currentUser.Id);
            var isCreator = task.CreatedById == currentUser.Id;
            var isAssigned = task.AssignedToId == currentUser.Id;

            if (!isAdmin && !isMember && !isCreator && !isAssigned)
            {
                return Forbid();
            }

            // Create the comment
            var comment = new TaskComment
            {
                Content = model.Content,
                CreatedAt = DateTime.Now,
                TaskId = model.TaskId,
                UserId = currentUser.Id
            };

            _context.TaskComments.Add(comment);
            await _context.SaveChangesAsync();

            // Process tagged users
            if (model.TaggedUserIds != null && model.TaggedUserIds.Any())
            {
                foreach (var userId in model.TaggedUserIds)
                {
                    // Create a mention record
                    var mention = new CommentMention
                    {
                        CommentId = comment.Id,
                        MentionedUserId = userId,
                        IsRead = false
                    };
                    _context.CommentMentions.Add(mention);

                    // Create a notification for the mentioned user
                    var notification = new Notification
                    {
                        Content = $"{currentUser.FirstName} {currentUser.LastName} mentioned you in a comment",
                        CreatedAt = DateTime.Now,
                        UserId = userId,
                        Type = NotificationType.Mention,
                        TaskId = task.Id,
                        CommentId = comment.Id,
                        IsRead = false,
                        Link = Url.Action("Details", "Task", new { id = task.Id })
                    };
                    _context.Notifications.Add(notification);
                    
                    // Send real-time notification
                    await NotificationService.SendRealTimeNotification(_hubContext, notification);
                }

                await _context.SaveChangesAsync();
            }

            // Create a notification for task assignee about new comment (if assignee is not commenter)
            if (!string.IsNullOrEmpty(task.AssignedToId) && task.AssignedToId != currentUser.Id)
            {
                var notification = new Notification
                {
                    Content = $"{currentUser.FirstName} {currentUser.LastName} commented on task '{task.Title}'",
                    CreatedAt = DateTime.Now,
                    UserId = task.AssignedToId,
                    Type = NotificationType.CommentAdded,
                    TaskId = task.Id,
                    CommentId = comment.Id,
                    IsRead = false,
                    Link = Url.Action("Details", "Task", new { id = task.Id })
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                
                // Send real-time notification
                await NotificationService.SendRealTimeNotification(_hubContext, notification);
            }

            // Also notify task creator if they're not the assignee or commenter
            if (task.CreatedById != currentUser.Id && task.CreatedById != task.AssignedToId)
            {
                var notification = new Notification
                {
                    Content = $"{currentUser.FirstName} {currentUser.LastName} commented on task '{task.Title}'",
                    CreatedAt = DateTime.Now,
                    UserId = task.CreatedById,
                    Type = NotificationType.CommentAdded,
                    TaskId = task.Id,
                    CommentId = comment.Id,
                    IsRead = false,
                    Link = Url.Action("Details", "Task", new { id = task.Id })
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            // Return the updated comments list
            return RedirectToAction("Details", "Task", new { id = model.TaskId });
        }

        // POST: Comment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string content, List<string> taggedUserIds)
        {
            var comment = await _context.TaskComments
                .Include(c => c.Task)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            // Only the comment author or an admin can edit a comment
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            if (comment.UserId != currentUser.Id && !isAdmin)
            {
                return Forbid();
            }

            comment.Content = content;
            _context.Update(comment);

            // Remove old mentions
            var oldMentions = await _context.CommentMentions
                .Where(m => m.CommentId == id)
                .ToListAsync();
            _context.CommentMentions.RemoveRange(oldMentions);

            // Process tagged users
            if (taggedUserIds != null && taggedUserIds.Any())
            {
                foreach (var userId in taggedUserIds)
                {
                    // Create a mention record
                    var mention = new CommentMention
                    {
                        CommentId = comment.Id,
                        MentionedUserId = userId,
                        IsRead = false
                    };
                    _context.CommentMentions.Add(mention);

                    // Create a notification for the mentioned user
                    var notification = new Notification
                    {
                        Content = $"{currentUser.FirstName} {currentUser.LastName} mentioned you in a comment",
                        CreatedAt = DateTime.Now,
                        UserId = userId,
                        Type = NotificationType.Mention,
                        TaskId = comment.Task.Id,
                        CommentId = comment.Id,
                        IsRead = false,
                        Link = Url.Action("Details", "Task", new { id = comment.Task.Id })
                    };
                    _context.Notifications.Add(notification);
                }
            }

            await _context.SaveChangesAsync();

            // Chuyển về trang Task Details thay vì trả về GetComments
            return RedirectToAction("Details", "Task", new { id = comment.TaskId });
        }

        // POST: Comment/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.TaskComments.FindAsync(id);
            
            if (comment == null)
            {
                return NotFound();
            }

            // Only the comment author or an admin can delete a comment
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            if (comment.UserId != currentUser.Id && !isAdmin)
            {
                return Forbid();
            }

            int taskId = comment.TaskId;

            _context.TaskComments.Remove(comment);
            await _context.SaveChangesAsync();

            // Chuyển về trang Task Details thay vì trả về GetComments
            return RedirectToAction("Details", "Task", new { id = taskId });
        }
        
        // POST: Comment/GetMentionSuggestions
        [HttpPost]
        public async Task<IActionResult> GetMentionSuggestions(int taskId, string query)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return NotFound();
            }

            // Get project members matching the query
            var members = await _context.ProjectMembers
                .Include(pm => pm.User)
                .Where(pm => pm.ProjectId == task.ProjectId && 
                       (pm.User.UserName.Contains(query) || 
                        pm.User.FirstName.Contains(query) || 
                        pm.User.LastName.Contains(query)))
                .Select(pm => new
                {
                    id = pm.User.Id,
                    username = pm.User.UserName,
                    name = $"{pm.User.FirstName} {pm.User.LastName}",
                    profilePicture = pm.User.ProfilePicture ?? "/images/default-profile.png"
                })
                .ToListAsync();

            return Json(members);
        }
    }
} 