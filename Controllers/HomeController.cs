using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.ViewModels;
using System.Diagnostics;

namespace ProjectManagementSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(
        ILogger<HomeController> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return View();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

        // Get active projects count
        ViewBag.ActiveProjectsCount = await _context.Projects
            .Where(p => p.Status == ProjectStatus.InProgress)
            .CountAsync();

        // Get total tasks count
        ViewBag.TotalTasksCount = await _context.ProjectTasks.CountAsync();

        // Get active teams count
        ViewBag.ActiveTeamsCount = await _context.Teams.CountAsync();

        // Get total team members count
        ViewBag.TotalMembersCount = await _context.TeamMembers.CountAsync();

        // Get recent projects
        var recentProjects = await _context.Projects
            .Include(p => p.ProjectMembers)
            .ThenInclude(pm => pm.User)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new
            {
                p.Id,
                p.Name,
                Status = p.Status.ToString(),
                EndDate = p.EndDate,
                Progress = p.Tasks.Any() ? 
                    (int)((double)p.Tasks.Count(t => t.Status == ProjectTaskStatus.Completed) / p.Tasks.Count() * 100) : 0,
                TeamMembers = p.ProjectMembers.Select(pm => new
                {
                    Name = $"{pm.User.FirstName} {pm.User.LastName}",
                    Initials = $"{pm.User.FirstName[0]}{pm.User.LastName[0]}"
                }).ToList(),
                IsMember = isAdmin || p.ProjectMembers.Any(pm => pm.UserId == currentUser.Id)
            })
            .ToListAsync();

        ViewBag.RecentProjects = recentProjects;

        // Get recent tasks
        var recentTasks = await _context.ProjectTasks
            .Include(t => t.Project)
            .ThenInclude(p => p.ProjectMembers)
            .Include(t => t.AssignedTo)
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new
            {
                t.Id,
                t.Title,
                ProjectName = t.Project.Name,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                Deadline = t.Deadline,
                AssignedTo = t.AssignedTo != null ? 
                    $"{t.AssignedTo.FirstName} {t.AssignedTo.LastName}" : "Unassigned",
                AssignedToInitials = t.AssignedTo != null ? 
                    $"{t.AssignedTo.FirstName[0]}{t.AssignedTo.LastName[0]}" : "U",
                IsMember = isAdmin || t.AssignedToId == currentUser.Id || t.Project.ProjectMembers.Any(pm => pm.UserId == currentUser.Id)
            })
            .ToListAsync();

        ViewBag.RecentTasks = recentTasks;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
