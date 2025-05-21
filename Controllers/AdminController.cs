using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.ViewModels;

namespace ProjectManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Users(string searchString)
        {
            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => 
                    u.Email.Contains(searchString) || 
                    u.FirstName.Contains(searchString) || 
                    u.LastName.Contains(searchString));
            }

            var usersList = await users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in usersList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userViewModel = new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Role = user.Role,
                    Roles = roles.ToList()
                };
                userViewModels.Add(userViewModel);
            }

            return View(userViewModels);
        }

        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var roles = _roleManager.Roles.ToList();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                Role = user.Role,
                Roles = roles.Select(r => new RoleViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    IsSelected = userRoles.Contains(r.Name)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.IsActive = model.IsActive;
            user.Role = model.Role;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Update roles
            var userRoles = await _userManager.GetRolesAsync(user);
            
            // Sync ASP.NET Identity Role with our custom Role enum
            if (model.Role == UserRole.Admin && !userRoles.Contains("Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                await _userManager.RemoveFromRoleAsync(user, "User");
            }
            else if (model.Role == UserRole.User && !userRoles.Contains("User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            }
            
            // Update other roles
            foreach (var role in model.Roles)
            {
                // Skip Admin and User roles as they're handled by the Role enum
                if (role.Name == "Admin" || role.Name == "User")
                    continue;
                    
                if (userRoles.Contains(role.Name) && !role.IsSelected)
                {
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else if (!userRoles.Contains(role.Name) && role.IsSelected)
                {
                    await _userManager.AddToRoleAsync(user, role.Name);
                }
            }

            TempData["StatusMessage"] = "User updated successfully";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the user.";
                return RedirectToAction(nameof(Users));
            }

            TempData["StatusMessage"] = "User deleted successfully";
            return RedirectToAction(nameof(Users));
        }
        
        // GET: Admin/TestNotification
        public async Task<IActionResult> TestNotification()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }
    }
} 