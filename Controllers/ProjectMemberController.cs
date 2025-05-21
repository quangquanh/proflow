using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.ViewModels;

namespace ProjectManagementSystem.Controllers
{
    [Authorize]
    public class ProjectMemberController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectMemberController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.CreatedBy)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.User)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var isMember = project.ProjectMembers.Any(pm => pm.UserId == currentUser.Id);

            if (!isAdmin && !isMember)
            {
                return Forbid();
            }

            var members = project.ProjectMembers.Select(pm => new ProjectMemberViewModel
            {
                Id = pm.Id,
                ProjectId = pm.ProjectId,
                UserId = pm.UserId,
                UserName = $"{pm.User.FirstName} {pm.User.LastName}",
                UserEmail = pm.User.Email,
                Role = pm.Role,
                JoinedAt = pm.JoinedAt
            }).ToList();

            ViewData["ProjectId"] = projectId;
            ViewData["ProjectName"] = project.Name;
            ViewData["IsOwner"] = project.ProjectMembers.Any(pm => 
                pm.UserId == currentUser.Id && pm.Role == ProjectRole.Owner);

            return View(members);
        }

        public async Task<IActionResult> Add(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            // Get all users except those who are already members
            var existingMemberIds = project.ProjectMembers.Select(pm => pm.UserId).ToList();
            var availableUsers = await _userManager.Users
                .Where(u => !existingMemberIds.Contains(u.Id))
                .Select(u => new
                {
                    Id = u.Id,
                    Email = u.Email,
                    Name = $"{u.FirstName} {u.LastName}"
                })
                .ToListAsync();

            var model = new AddProjectMemberViewModel
            {
                ProjectId = projectId
            };

            ViewBag.AvailableUsers = new SelectList(availableUsers, "Email", "Name");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddProjectMemberViewModel model)
        {
            if (ModelState.IsValid)
            {
                var project = await _context.Projects
                    .Include(p => p.ProjectMembers)
                    .FirstOrDefaultAsync(p => p.Id == model.ProjectId);

                if (project == null)
                {
                    return NotFound();
                }

                var currentUser = await _userManager.GetUserAsync(User);
                var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
                var isOwner = project.ProjectMembers.Any(pm => 
                    pm.UserId == currentUser.Id && pm.Role == ProjectRole.Owner);

                if (!isAdmin && !isOwner)
                {
                    return Forbid();
                }

                var user = await _userManager.FindByEmailAsync(model.UserEmail);
                if (user == null)
                {
                    ModelState.AddModelError("UserEmail", "User not found");
                    return View(model);
                }

                var existingMember = await _context.ProjectMembers
                    .FirstOrDefaultAsync(pm => pm.ProjectId == model.ProjectId && pm.UserId == user.Id);

                if (existingMember != null)
                {
                    ModelState.AddModelError("UserEmail", "User is already a member of this project");
                    return View(model);
                }

                var projectMember = new ProjectMember
                {
                    ProjectId = model.ProjectId,
                    UserId = user.Id,
                    Role = model.Role,
                    JoinedAt = DateTime.Now
                };

                _context.ProjectMembers.Add(projectMember);
                await _context.SaveChangesAsync();

                // Create a notification for the user being added to the project
                var notification = new Notification
                {
                    Content = $"You have been added to project '{project.Name}' as a {model.Role}",
                    CreatedAt = DateTime.Now,
                    UserId = user.Id,
                    Type = NotificationType.ProjectMemberAdded,
                    IsRead = false,
                    Link = Url.Action("Details", "Project", new { id = model.ProjectId })
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var projectMember = await _context.ProjectMembers
                .Include(pm => pm.Project)
                    .ThenInclude(p => p.ProjectMembers)
                .FirstOrDefaultAsync(pm => pm.Id == id);

            if (projectMember == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var isOwner = projectMember.Project.ProjectMembers.Any(pm => 
                pm.UserId == currentUser.Id && pm.Role == ProjectRole.Owner);

            if (!isAdmin && !isOwner)
            {
                return Forbid();
            }

            _context.ProjectMembers.Remove(projectMember);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { projectId = projectMember.ProjectId });
        }
    }
} 