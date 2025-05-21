using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.ViewModels;

namespace ProjectManagementSystem.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchString, ProjectStatus? status)
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var projectsQuery = _context.Projects
                .Include(p => p.CreatedBy)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.User)
                .AsQueryable();

            if (!isAdmin)
            {
                projectsQuery = projectsQuery.Where(p => 
                    p.CreatedById == user.Id || 
                    p.ProjectMembers.Any(pm => pm.UserId == user.Id));
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                projectsQuery = projectsQuery.Where(p => 
                    p.Name.Contains(searchString) || 
                    p.Description.Contains(searchString));
            }

            if (status.HasValue)
            {
                projectsQuery = projectsQuery.Where(p => p.Status == status.Value);
            }

            var projects = await projectsQuery.ToListAsync();
            return View(projects);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.CreatedBy)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.User)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.AssignedTo)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isMember = project.ProjectMembers.Any(pm => pm.UserId == user.Id);

            if (!isAdmin && !isMember)
            {
                return Forbid();
            }

            var isOwner = project.ProjectMembers.Any(pm => 
                pm.UserId == user.Id && pm.Role == ProjectRole.Owner);

            ViewData["IsOwner"] = isOwner;
            ViewData["IsAdmin"] = isAdmin;

            return View(project);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var project = new Project
                {
                    Name = model.Name,
                    Description = model.Description,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    CreatedById = user.Id,
                    Status = model.Status
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                // Add creator as project owner
                var projectMember = new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = user.Id,
                    Role = ProjectRole.Owner
                };

                _context.ProjectMembers.Add(projectMember);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isOwner = project.ProjectMembers.Any(pm => 
                pm.UserId == user.Id && pm.Role == ProjectRole.Owner);

            if (!isAdmin && !isOwner)
            {
                return Forbid();
            }

            var model = new ProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Status = project.Status
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var project = await _context.Projects
                    .Include(p => p.ProjectMembers)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    return NotFound();
                }

                var user = await _userManager.GetUserAsync(User);
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                var isOwner = project.ProjectMembers.Any(pm => 
                    pm.UserId == user.Id && pm.Role == ProjectRole.Owner);

                if (!isAdmin && !isOwner)
                {
                    return Forbid();
                }

                project.Name = model.Name;
                project.Description = model.Description;
                project.StartDate = model.StartDate;
                project.EndDate = model.EndDate;
                project.Status = model.Status;
                project.UpdatedAt = DateTime.Now;

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isOwner = project.ProjectMembers.Any(pm => 
                pm.UserId == user.Id && pm.Role == ProjectRole.Owner);

            if (!isAdmin && !isOwner)
            {
                return Forbid();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
} 