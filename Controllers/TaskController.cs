using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IEmailService _emailService;

        public TaskController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<NotificationHub> hubContext,
            IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
            _emailService = emailService;
        }

        // GET: Task/Board/5 (projectId)
        public async Task<IActionResult> Board(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .ThenInclude(pm => pm.User)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            // Check if user is a member of the project
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var isMember = project.ProjectMembers.Any(pm => pm.UserId == currentUser.Id);

            if (!isAdmin && !isMember)
            {
                return Forbid();
            }

            var tasks = await _context.ProjectTasks
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            var model = new TasksListViewModel
            {
                ProjectId = projectId,
                ProjectName = project.Name,
                NotStartedTasks = tasks.Where(t => t.Status == ProjectTaskStatus.NotStarted)
                    .Select(MapToViewModel)
                    .ToList(),
                InProgressTasks = tasks.Where(t => t.Status == ProjectTaskStatus.InProgress)
                    .Select(MapToViewModel)
                    .ToList(),
                CompletedTasks = tasks.Where(t => t.Status == ProjectTaskStatus.Completed)
                    .Select(MapToViewModel)
                    .ToList(),
                OnHoldTasks = tasks.Where(t => t.Status == ProjectTaskStatus.OnHold)
                    .Select(MapToViewModel)
                    .ToList()
            };

            return View(model);
        }

        // GET: Task/Create/5 (projectId)
        public async Task<IActionResult> Create(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .ThenInclude(pm => pm.User)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            // Check if user is a member of the project
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var isMember = project.ProjectMembers.Any(pm => pm.UserId == currentUser.Id);

            if (!isAdmin && !isMember)
            {
                return Forbid();
            }

            var projects = await _context.Projects.ToListAsync();
            var projectMembers = project.ProjectMembers.Select(pm => pm.User).ToList();

            var model = new CreateTaskViewModel
            {
                ProjectId = projectId,
                Projects = new SelectList(projects, "Id", "Name"),
                ProjectMembers = new SelectList(projectMembers, "Id", "FirstName", currentUser.Id)
            };

            return View(model);
        }

        // POST: Task/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTaskViewModel model)
        {
            try
            {
                // Remove Projects and ProjectMembers from ModelState to prevent validation errors
                ModelState.Remove("Projects");
                ModelState.Remove("ProjectMembers");

                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(x => x.Errors)
                        .Select(x => x.ErrorMessage));

                    TempData["ErrorMessage"] = $"Form validation failed: {errors}";
                }

                var currentUser = await _userManager.GetUserAsync(User);
                var project = await _context.Projects
                    .Include(p => p.ProjectMembers)
                    .FirstOrDefaultAsync(p => p.Id == model.ProjectId);

                if (project == null)
                {
                    TempData["ErrorMessage"] = "Project not found.";
                    return RedirectToAction(nameof(Board), new { projectId = model.ProjectId });
                }

                var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
                var isMember = project.ProjectMembers.Any(pm => pm.UserId == currentUser.Id);

                if (!isAdmin && !isMember)
                {
                    TempData["ErrorMessage"] = "You don't have access to this project.";
                    return RedirectToAction("Index", "Project");
                }

                // Create the task object
                var task = new ProjectTask
                {
                    Title = model.Title,
                    Description = model.Description,
                    Status = model.Status,
                    Priority = model.Priority,
                    CreatedAt = DateTime.Now,
                    Deadline = model.Deadline,
                    ProjectId = model.ProjectId,
                    CreatedById = currentUser.Id,
                    AssignedToId = model.AssignedToId
                };

                // First save the task without attachment
                _context.Add(task);
                await _context.SaveChangesAsync();

                // Create notification for task assignment if assigned to someone
                if (!string.IsNullOrEmpty(model.AssignedToId))
                {
                    var notification = new Notification
                    {
                        Content = $"Task '{model.Title}' has been assigned to you by {currentUser.FirstName} {currentUser.LastName}",
                        CreatedAt = DateTime.Now,
                        UserId = model.AssignedToId,
                        Type = NotificationType.TaskAssignment,
                        TaskId = task.Id,
                        IsRead = false,
                        Link = Url.Action("Details", "Task", new { id = task.Id })
                    };
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                    
                    // Send real-time notification
                    await NotificationService.SendRealTimeNotification(_hubContext, notification);

                    // Send email notification for task assignment
                    var assignedUser = await _userManager.FindByIdAsync(model.AssignedToId);
                    if (assignedUser != null && !string.IsNullOrEmpty(assignedUser.Email))
                    {
                        var emailSubject = $"New Task Assignment: {model.Title}";
                        var emailBody = $@"
                            <h2>New Task Assignment</h2>
                            <p>Hello {assignedUser.FirstName} {assignedUser.LastName},</p>
                            <p>You have been assigned a new task:</p>
                            <ul>
                                <li><strong>Task:</strong> {model.Title}</li>
                                <li><strong>Description:</strong> {model.Description}</li>
                                <li><strong>Priority:</strong> {model.Priority}</li>
                                <li><strong>Deadline:</strong> {model.Deadline:dd/MM/yyyy}</li>
                                <li><strong>Assigned By:</strong> {currentUser.FirstName} {currentUser.LastName}</li>
                            </ul>
                            <p>You can view the task details by clicking <a href='{Url.Action("Details", "Task", new { id = task.Id }, Request.Scheme)}'>here</a>.</p>
                            <p>Best regards,<br>Project Management System</p>";

                        await _emailService.SendEmailAsync(assignedUser.Email, emailSubject, emailBody);
                    }
                }

                // Then handle file upload
                if (model.Attachment != null && model.Attachment.Length > 0)
                {
                    try
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "tasks");
                        Directory.CreateDirectory(uploadsFolder); // Ensure directory exists

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.Attachment.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Attachment.CopyToAsync(fileStream);
                        }

                        // Update the task with attachment path
                        task.AttachmentPath = "/uploads/tasks/" + uniqueFileName;
                        _context.Update(task);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        // Task already saved, just log error about attachment
                        TempData["ErrorMessage"] = $"Task created but file upload failed: {ex.Message}";
                        return RedirectToAction(nameof(Board), new { projectId = model.ProjectId });
                    }
                }

                TempData["StatusMessage"] = "Task created successfully.";
                return RedirectToAction(nameof(Board), new { projectId = model.ProjectId });
            }
            catch (Exception ex)
            {
                // Repopulate form data
                var projects = await _context.Projects.ToListAsync();
                var projectMembers = await _context.Users.ToListAsync();

                model.Projects = new SelectList(projects, "Id", "Name");
                model.ProjectMembers = new SelectList(projectMembers, "Id", "FirstName");

                TempData["ErrorMessage"] = $"Error creating task: {ex.Message}";
                return View(model);
            }
        }

        // GET: Task/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == id);

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

            var projects = await _context.Projects.ToListAsync();
            var projectMembers = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == task.ProjectId)
                .Include(pm => pm.User)
                .Select(pm => pm.User)
                .ToListAsync();

            var model = new EditTaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                Deadline = task.Deadline,
                ProjectId = task.ProjectId,
                AssignedToId = task.AssignedToId,
                ExistingAttachmentPath = task.AttachmentPath,
                Projects = new SelectList(projects, "Id", "Name"),
                ProjectMembers = new SelectList(projectMembers, "Id", "FirstName")
            };
            

            return View(model);
        }

        // POST: Task/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditTaskViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

      
                try
                {
                    var originalTask = await _context.ProjectTasks
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Id == model.Id);
                    
                    var task = await _context.ProjectTasks.FindAsync(model.Id);
                    task.Title = model.Title;
                    task.Description = model.Description;
                    task.Deadline = model.Deadline;
                    
                    // Check if status changed - nếu thay đổi thì tạo thông báo
                    bool statusChanged = task.Status != model.Status;
                    ProjectTaskStatus oldStatus = task.Status;
                    task.Status = model.Status;
                    
                    task.Priority = model.Priority;
                    task.ProjectId = model.ProjectId;
                    
                    // Check if assignee changed - nếu thay đổi thì tạo thông báo
                    bool assigneeChanged = task.AssignedToId != model.AssignedToId;
                    string oldAssigneeId = task.AssignedToId;
                    task.AssignedToId = model.AssignedToId;

                    // Process attachment if exists
                    if (model.Attachment != null && model.Attachment.Length > 0)
                    {
                        // If there's an existing attachment, delete it
                        if (!string.IsNullOrEmpty(task.AttachmentPath) && System.IO.File.Exists(task.AttachmentPath))
                        {
                            System.IO.File.Delete(task.AttachmentPath);
                        }
                        
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "tasks");
                        Directory.CreateDirectory(uploadsFolder); // Ensure directory exists
                        
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Attachment.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Attachment.CopyToAsync(fileStream);
                        }
                        
                        task.AttachmentPath = "/uploads/tasks/" + uniqueFileName;
                    }
                    
                    _context.Update(task);
                    await _context.SaveChangesAsync();
                    
                    var currentUser = await _userManager.GetUserAsync(User);
                    
                    // Create notification if status changed
                    if (statusChanged && !string.IsNullOrEmpty(task.AssignedToId))
                    {
                        var notification = new Notification
                        {
                            Content = $"Task '{task.Title}' status changed from {oldStatus} to {task.Status}",
                            CreatedAt = DateTime.Now,
                            UserId = task.AssignedToId,
                            Type = NotificationType.TaskStatusChanged,
                            TaskId = task.Id,
                            IsRead = false,
                            Link = Url.Action("Details", "Task", new { id = task.Id })
                        };
                        _context.Notifications.Add(notification);
                        
                        // Send real-time notification
                        await NotificationService.SendRealTimeNotification(_hubContext, notification);

                        // Send email notification for status change
                        if (task.AssignedTo != null && !string.IsNullOrEmpty(task.AssignedTo.Email))
                        {
                            var emailSubject = $"Task Status Update: {task.Title}";
                            var emailBody = $@"
                                <h2>Task Status Update</h2>
                                <p>Hello {task.AssignedTo.FirstName} {task.AssignedTo.LastName},</p>
                                <p>The status of your assigned task has been updated:</p>
                                <ul>
                                    <li><strong>Task:</strong> {task.Title}</li>
                                    <li><strong>Old Status:</strong> {oldStatus}</li>
                                    <li><strong>New Status:</strong> {task.Status}</li>
                                    <li><strong>Updated By:</strong> {currentUser.FirstName} {currentUser.LastName}</li>
                                </ul>
                                <p>You can view the task details by clicking <a href='{Url.Action("Details", "Task", new { id = task.Id }, Request.Scheme)}'>here</a>.</p>
                                <p>Best regards,<br>Project Management System</p>";

                            await _emailService.SendEmailAsync(task.AssignedTo.Email, emailSubject, emailBody);
                        }
                    }
                    
                    // Create notification if assignee changed
                    if (assigneeChanged && !string.IsNullOrEmpty(task.AssignedToId))
                    {
                        var notification = new Notification
                        {
                            Content = $"Task '{task.Title}' has been assigned to you by {currentUser.FirstName} {currentUser.LastName}",
                            CreatedAt = DateTime.Now,
                            UserId = task.AssignedToId,
                            Type = NotificationType.TaskAssignment,
                            TaskId = task.Id,
                            IsRead = false,
                            Link = Url.Action("Details", "Task", new { id = task.Id })
                        };
                        _context.Notifications.Add(notification);
                        
                        // Send real-time notification
                        await NotificationService.SendRealTimeNotification(_hubContext, notification);

                        // Send email notification for task reassignment
                        var newAssignee = await _userManager.FindByIdAsync(task.AssignedToId);
                        if (newAssignee != null && !string.IsNullOrEmpty(newAssignee.Email))
                        {
                            var emailSubject = $"Task Reassignment: {task.Title}";
                            var emailBody = $@"
                                <h2>Task Reassignment</h2>
                                <p>Hello {newAssignee.FirstName} {newAssignee.LastName},</p>
                                <p>You have been assigned a task:</p>
                                <ul>
                                    <li><strong>Task:</strong> {task.Title}</li>
                                    <li><strong>Description:</strong> {task.Description}</li>
                                    <li><strong>Priority:</strong> {task.Priority}</li>
                                    <li><strong>Deadline:</strong> {task.Deadline:dd/MM/yyyy}</li>
                                    <li><strong>Assigned By:</strong> {currentUser.FirstName} {currentUser.LastName}</li>
                                </ul>
                                <p>You can view the task details by clicking <a href='{Url.Action("Details", "Task", new { id = task.Id }, Request.Scheme)}'>here</a>.</p>
                                <p>Best regards,<br>Project Management System</p>";

                            await _emailService.SendEmailAsync(newAssignee.Email, emailSubject, emailBody);
                        }
                    }

                    // Send email notification for other task updates
                    if (!statusChanged && !assigneeChanged && task.AssignedTo != null && !string.IsNullOrEmpty(task.AssignedTo.Email))
                    {
                        var emailSubject = $"Task Update: {task.Title}";
                        var emailBody = $@"
                            <h2>Task Update</h2>
                            <p>Hello {task.AssignedTo.FirstName} {task.AssignedTo.LastName},</p>
                            <p>Your assigned task has been updated:</p>
                            <ul>
                                <li><strong>Task:</strong> {task.Title}</li>
                                <li><strong>Description:</strong> {task.Description}</li>
                                <li><strong>Priority:</strong> {task.Priority}</li>
                                <li><strong>Deadline:</strong> {task.Deadline:dd/MM/yyyy}</li>
                                <li><strong>Updated By:</strong> {currentUser.FirstName} {currentUser.LastName}</li>
                            </ul>
                            <p>You can view the task details by clicking <a href='{Url.Action("Details", "Task", new { id = task.Id }, Request.Scheme)}'>here</a>.</p>
                            <p>Best regards,<br>Project Management System</p>";

                        await _emailService.SendEmailAsync(task.AssignedTo.Email, emailSubject, emailBody);
                    }
                    
                    await _context.SaveChangesAsync();
                    
                    TempData["StatusMessage"] = "Task updated successfully";
                    return RedirectToAction(nameof(Details), new { id = task.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            

            // If we got this far, something failed, redisplay form
            var projects = await _context.Projects.ToListAsync();
            var projectMembers = await _context.Users.ToListAsync();

            model.Projects = new SelectList(projects, "Id", "Name");
            model.ProjectMembers = new SelectList(projectMembers, "Id", "FirstName");

            return View(model);
        }

        // GET: Task/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == id);

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

            var model = MapToViewModel(task);

            return View(model);
        }

        // POST: Task/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Mentions)
                .FirstOrDefaultAsync(t => t.Id == id);
                
            if (task == null)
            {
                return NotFound();
            }

            // Check if user has access to delete the task
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var isCreator = task.CreatedById == currentUser.Id;

            if (!isAdmin && !isCreator)
            {
                return Forbid();
            }

            int projectId = task.ProjectId;

            // Delete related notifications first
            var notifications = await _context.Notifications
                .Where(n => n.TaskId == id)
                .ToListAsync();
            _context.Notifications.RemoveRange(notifications);

            // Delete all comments and their mentions
            foreach (var comment in task.Comments)
            {
                _context.CommentMentions.RemoveRange(comment.Mentions);
            }
            _context.TaskComments.RemoveRange(task.Comments);

            // Delete attachment if exists
            if (!string.IsNullOrEmpty(task.AttachmentPath))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, task.AttachmentPath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.ProjectTasks.Remove(task);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Board), new { projectId });
        }

        // POST: Task/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(UpdateTaskStatusViewModel model)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.AssignedTo)
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
                return Json(new { success = false, message = "You don't have permission to update this task" });
            }

            var oldStatus = task.Status;
            task.Status = model.NewStatus;
            _context.Update(task);
            await _context.SaveChangesAsync();

            // Create and send real-time notification for status change
            if (!string.IsNullOrEmpty(task.AssignedToId) && task.AssignedToId != currentUser.Id)
            {
                var notification = new Notification
                {
                    Content = $"Task '{task.Title}' status changed from {oldStatus} to {task.Status} by {currentUser.FirstName} {currentUser.LastName}",
                    CreatedAt = DateTime.Now,
                    UserId = task.AssignedToId,
                    Type = NotificationType.TaskStatusChanged,
                    TaskId = task.Id,
                    IsRead = false,
                    Link = Url.Action("Details", "Task", new { id = task.Id })
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                
                // Send real-time notification
                await NotificationService.SendRealTimeNotification(_hubContext, notification);

                // Send email notification
                if (task.AssignedTo != null && !string.IsNullOrEmpty(task.AssignedTo.Email))
                {
                    var emailSubject = $"Task Status Update: {task.Title}";
                    var emailBody = $@"
                        <h2>Task Status Update</h2>
                        <p>Hello {task.AssignedTo.FirstName} {task.AssignedTo.LastName},</p>
                        <p>The status of your assigned task has been updated:</p>
                        <ul>
                            <li><strong>Task:</strong> {task.Title}</li>
                            <li><strong>Old Status:</strong> {oldStatus}</li>
                            <li><strong>New Status:</strong> {task.Status}</li>
                            <li><strong>Updated By:</strong> {currentUser.FirstName} {currentUser.LastName}</li>
                        </ul>
                        <p>You can view the task details by clicking <a href='{Url.Action("Details", "Task", new { id = task.Id }, Request.Scheme)}'>here</a>.</p>
                        <p>Best regards,<br>Project Management System</p>";

                    await _emailService.SendEmailAsync(task.AssignedTo.Email, emailSubject, emailBody);
                }
            }

            return Json(new { success = true });
        }

        // GET: Task/GetTasksByProject/5 (API method for AJAX)
        public async Task<IActionResult> GetTasksByProject(int projectId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var isMember = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == currentUser.Id);

            if (!isAdmin && !isMember)
            {
                return Json(new { success = false, message = "You don't have permission to view these tasks" });
            }

            var tasks = await _context.ProjectTasks
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            var tasksByStatus = new
            {
                notStarted = tasks.Where(t => t.Status == ProjectTaskStatus.NotStarted)
                    .Select(MapToViewModel),
                inProgress = tasks.Where(t => t.Status == ProjectTaskStatus.InProgress)
                    .Select(MapToViewModel),
                completed = tasks.Where(t => t.Status == ProjectTaskStatus.Completed)
                    .Select(MapToViewModel),
                onHold = tasks.Where(t => t.Status == ProjectTaskStatus.OnHold)
                    .Select(MapToViewModel)
            };

            return Json(tasksByStatus);
        }

        // GET: Task/Calendar/5 (projectId)
        public async Task<IActionResult> Calendar(int projectId, string viewMode = "month")
        {
            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .ThenInclude(pm => pm.User)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            // Check if user is a member of the project
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var isMember = project.ProjectMembers.Any(pm => pm.UserId == currentUser.Id);

            if (!isAdmin && !isMember)
            {
                return Forbid();
            }

            var tasks = await _context.ProjectTasks
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            var model = new TaskCalendarViewModel
            {
                ProjectId = projectId,
                ProjectName = project.Name,
                Tasks = tasks.Select(MapToViewModel).ToList(),
                ViewMode = viewMode
            };

            return View(model);
        }

        // GET: Task/GetCalendarTasks/5 (API method for AJAX)
        public async Task<IActionResult> GetCalendarTasks(int projectId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var isMember = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == currentUser.Id);

            if (!isAdmin && !isMember)
            {
                return Json(new { success = false, message = "You don't have permission to view these tasks" });
            }

            var tasks = await _context.ProjectTasks
                .Include(t => t.AssignedTo)
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            // Log the number of tasks found
            System.Diagnostics.Debug.WriteLine($"Found {tasks.Count} tasks for project {projectId}");

            var calendarTasks = tasks.Select(t => new
            {
                id = t.Id,
                title = t.Title,
                start = t.CreatedAt.ToString("yyyy-MM-dd"), // Use CreatedAt as start date
                end = t.Deadline.ToString("yyyy-MM-dd"), // Use Deadline as end date
                allDay = true, // Make events all-day events
                backgroundColor = t.Status switch
                {
                    ProjectTaskStatus.NotStarted => "#6c757d", // secondary
                    ProjectTaskStatus.InProgress => "#0d6efd", // primary
                    ProjectTaskStatus.Completed => "#198754", // success
                    ProjectTaskStatus.OnHold => "#ffc107", // warning
                    _ => "#6c757d" // secondary
                },
                borderColor = t.Priority switch
                {
                    Priority.High => "#dc3545", // danger
                    Priority.Medium => "#fd7e14", // orange
                    Priority.Low => "#0dcaf0", // info
                    _ => "#6c757d" // secondary
                },
                url = Url.Action("Details", "Task", new { id = t.Id }),
                description = $"Assigned to: {(t.AssignedTo != null ? $"{t.AssignedTo.FirstName} {t.AssignedTo.LastName}" : "Not assigned")}\nStatus: {t.Status}\nPriority: {t.Priority}"
            }).ToList();

            // Log the transformed calendar tasks
            System.Diagnostics.Debug.WriteLine($"Transformed {calendarTasks.Count} tasks for calendar display");

            return Json(calendarTasks);
        }

        // Helper method to map a ProjectTask to a TaskViewModel
        private TaskViewModel MapToViewModel(ProjectTask task)
        {
            if (task == null)
            {
                return new TaskViewModel();
            }

            return new TaskViewModel
            {
                Id = task.Id,
                Title = task.Title ?? string.Empty,
                Description = task.Description ?? string.Empty,
                Status = task.Status,
                Priority = task.Priority,
                CreatedAt = task.CreatedAt,
                Deadline = task.Deadline,
                ProjectId = task.ProjectId,
                ProjectName = task.Project?.Name ?? string.Empty,
                CreatedByName = task.CreatedBy != null ? $"{task.CreatedBy.FirstName} {task.CreatedBy.LastName}" : string.Empty,
                AssignedToName = task.AssignedTo != null ? $"{task.AssignedTo.FirstName} {task.AssignedTo.LastName}" : string.Empty,
                AssignedToId = task.AssignedToId,
                AttachmentPath = task.AttachmentPath ?? string.Empty,
                CommentCount = _context.TaskComments.Count(c => c.TaskId == task.Id)
            };
        }

        private bool TaskExists(int id)
        {
            return _context.ProjectTasks.Any(e => e.Id == id);
        }
    }
} 