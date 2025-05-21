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
    public class TeamController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeamController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Team
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            var teams = await _context.Teams
                .Include(t => t.CreatedBy)
                .Include(t => t.TeamMembers)
                .ThenInclude(tm => tm.User)
                .ToListAsync();

            // Filter teams based on user role
            if (!isAdmin)
            {
                teams = teams.Where(t => 
                    t.CreatedById == currentUser.Id || 
                    t.TeamMembers.Any(tm => tm.UserId == currentUser.Id)
                ).ToList();
            }

            return View(teams);
        }

        // GET: Team/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Team/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeamViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var team = new Team
                {
                    Name = model.Name,
                    Description = model.Description,
                    CreatedAt = DateTime.Now,
                    CreatedById = currentUser.Id
                };

                _context.Add(team);
                await _context.SaveChangesAsync();

                // Add creator as team lead
                var teamMember = new TeamMember
                {
                    TeamId = team.Id,
                    UserId = currentUser.Id,
                    Role = TeamRole.TeamLead,
                    JoinedAt = DateTime.Now
                };

                _context.Add(teamMember);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Team/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                .ThenInclude(tm => tm.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var isTeamLead = team.TeamMembers.Any(tm => 
                tm.UserId == currentUser.Id && tm.Role == TeamRole.TeamLead);

            if (!isAdmin && !isTeamLead)
            {
                return Forbid();
            }

            var model = new TeamViewModel
            {
                Id = team.Id,
                Name = team.Name,
                Description = team.Description,
                TeamMembers = team.TeamMembers.Select(tm => new TeamMemberViewModel
                {
                    Id = tm.Id,
                    UserId = tm.UserId,
                    UserName = $"{tm.User.FirstName} {tm.User.LastName}",
                    Role = tm.Role
                }).ToList()
            };

            ViewBag.AvailableUsers = new SelectList(
                await _context.Users.ToListAsync(),
                "Id",
                "FirstName"
            );

            return View(model);
        }

        // POST: Team/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TeamViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var team = await _context.Teams
                        .Include(t => t.TeamMembers)
                        .FirstOrDefaultAsync(t => t.Id == id);

                    if (team == null)
                    {
                        return NotFound();
                    }

                    var currentUser = await _userManager.GetUserAsync(User);
                    var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
                    var isTeamLead = team.TeamMembers.Any(tm => 
                        tm.UserId == currentUser.Id && tm.Role == TeamRole.TeamLead);

                    if (!isAdmin && !isTeamLead)
                    {
                        return Forbid();
                    }

                    team.Name = model.Name;
                    team.Description = model.Description;

                    _context.Update(team);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: Team/AddMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(AddTeamMemberViewModel model)
        {
            if (ModelState.IsValid)
            {
                var team = await _context.Teams
                    .Include(t => t.TeamMembers)
                    .FirstOrDefaultAsync(t => t.Id == model.TeamId);

                if (team == null)
                {
                    return NotFound();
                }

                var currentUser = await _userManager.GetUserAsync(User);
                var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
                var isTeamLead = team.TeamMembers.Any(tm => 
                    tm.UserId == currentUser.Id && tm.Role == TeamRole.TeamLead);

                if (!isAdmin && !isTeamLead)
                {
                    return Forbid();
                }

                // Check if user is already a member
                if (team.TeamMembers.Any(tm => tm.UserId == model.UserId))
                {
                    return Json(new { success = false, message = "User is already a member of this team" });
                }

                var teamMember = new TeamMember
                {
                    TeamId = model.TeamId,
                    UserId = model.UserId,
                    Role = model.Role,
                    JoinedAt = DateTime.Now
                };

                _context.Add(teamMember);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Invalid model state" });
        }

        // POST: Team/RemoveMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int teamId, string userId)
        {
            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var isTeamLead = team.TeamMembers.Any(tm => 
                tm.UserId == currentUser.Id && tm.Role == TeamRole.TeamLead);

            if (!isAdmin && !isTeamLead)
            {
                return Forbid();
            }

            var teamMember = team.TeamMembers.FirstOrDefault(tm => tm.UserId == userId);
            if (teamMember == null)
            {
                return NotFound();
            }

            // Prevent removing the last team lead
            if (teamMember.Role == TeamRole.TeamLead && 
                team.TeamMembers.Count(tm => tm.Role == TeamRole.TeamLead) <= 1)
            {
                return Json(new { success = false, message = "Cannot remove the last team lead" });
            }

            _context.Remove(teamMember);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.Id == id);
        }
    }
} 