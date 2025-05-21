using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Models.API;
using System.Net;
using System.Security.Claims;

namespace ProjectManagementSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeamsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Lấy danh sách tất cả đội nhóm
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTeams()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var query = _context.Teams.AsQueryable();

            // Nếu không phải admin, chỉ lấy các đội nhóm mà user là creator hoặc member
            if (!isAdmin)
            {
                query = query.Where(t => 
                    t.CreatedById == userId || 
                    t.TeamMembers.Any(tm => tm.UserId == userId));
            }

            var teams = await query
                .Include(t => t.CreatedBy)
                .Include(t => t.TeamMembers)
                    .ThenInclude(tm => tm.User)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(ApiResponse.SuccessResponse("Lấy danh sách đội nhóm thành công", teams));
        }

        /// <summary>
        /// Lấy thông tin chi tiết đội nhóm
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeam(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var team = await _context.Teams
                .Include(t => t.CreatedBy)
                .Include(t => t.TeamMembers)
                    .ThenInclude(tm => tm.User)
                .Include(t => t.ProjectTeams)
                    .ThenInclude(pt => pt.Project)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Đội nhóm không tồn tại", HttpStatusCode.NotFound));
            }

            // Kiểm tra quyền truy cập
            if (!isAdmin && 
                team.CreatedById != userId && 
                !team.TeamMembers.Any(tm => tm.UserId == userId))
            {
                return Forbid();
            }

            return Ok(ApiResponse.SuccessResponse("Lấy thông tin đội nhóm thành công", team));
        }

        /// <summary>
        /// Tạo đội nhóm mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromBody] Team team)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Dữ liệu không hợp lệ", HttpStatusCode.BadRequest, ModelState));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            team.CreatedById = userId;
            team.CreatedAt = DateTime.Now;

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // Tự động thêm người tạo vào đội nhóm với vai trò TeamLead
            var teamMember = new TeamMember
            {
                TeamId = team.Id,
                UserId = userId,
                Role = TeamRole.TeamLead,
                JoinedAt = DateTime.Now
            };

            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, 
                ApiResponse.SuccessResponse("Tạo đội nhóm thành công", team));
        }

        /// <summary>
        /// Cập nhật thông tin đội nhóm
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeam(int id, [FromBody] Team team)
        {
            if (id != team.Id)
            {
                return BadRequest(ApiResponse.ErrorResponse("ID không khớp", HttpStatusCode.BadRequest));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Dữ liệu không hợp lệ", HttpStatusCode.BadRequest, ModelState));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var existingTeam = await _context.Teams
                .Include(t => t.TeamMembers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existingTeam == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Đội nhóm không tồn tại", HttpStatusCode.NotFound));
            }

            // Kiểm tra quyền chỉnh sửa (chỉ creator, admin hoặc team lead mới có quyền)
            var isTeamLead = existingTeam.TeamMembers.Any(tm => tm.UserId == userId && tm.Role == TeamRole.TeamLead);
            if (!isAdmin && existingTeam.CreatedById != userId && !isTeamLead)
            {
                return Forbid();
            }

            existingTeam.Name = team.Name;
            existingTeam.Description = team.Description;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeamExists(id))
                {
                    return NotFound(ApiResponse.ErrorResponse("Đội nhóm không tồn tại", HttpStatusCode.NotFound));
                }
                else
                {
                    throw;
                }
            }

            return Ok(ApiResponse.SuccessResponse("Cập nhật đội nhóm thành công", existingTeam));
        }

        /// <summary>
        /// Xóa đội nhóm
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Đội nhóm không tồn tại", HttpStatusCode.NotFound));
            }

            // Kiểm tra quyền xóa (chỉ creator hoặc admin)
            if (!isAdmin && team.CreatedById != userId)
            {
                return Forbid();
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.SuccessResponse("Xóa đội nhóm thành công"));
        }

        /// <summary>
        /// Thêm thành viên vào đội nhóm
        /// </summary>
        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddMember(int id, [FromBody] TeamMember member)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Đội nhóm không tồn tại", HttpStatusCode.NotFound));
            }

            // Kiểm tra quyền thêm thành viên
            var isTeamLead = team.TeamMembers.Any(tm => tm.UserId == userId && tm.Role == TeamRole.TeamLead);
            if (!isAdmin && team.CreatedById != userId && !isTeamLead)
            {
                return Forbid();
            }

            // Kiểm tra user có tồn tại không
            var user = await _userManager.FindByIdAsync(member.UserId);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Người dùng không tồn tại", HttpStatusCode.NotFound));
            }

            // Kiểm tra user có trong đội nhóm chưa
            if (team.TeamMembers.Any(tm => tm.UserId == member.UserId))
            {
                return BadRequest(ApiResponse.ErrorResponse("Người dùng đã là thành viên của đội nhóm", HttpStatusCode.BadRequest));
            }

            member.TeamId = id;
            member.JoinedAt = DateTime.Now;

            _context.TeamMembers.Add(member);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.SuccessResponse("Thêm thành viên thành công", member));
        }

        /// <summary>
        /// Cập nhật vai trò thành viên trong đội nhóm
        /// </summary>
        [HttpPut("{id}/members/{userId}")]
        public async Task<IActionResult> UpdateMemberRole(int id, string userId, [FromBody] TeamRole role)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Đội nhóm không tồn tại", HttpStatusCode.NotFound));
            }

            // Kiểm tra quyền cập nhật vai trò
            var isTeamLead = team.TeamMembers.Any(tm => tm.UserId == currentUserId && tm.Role == TeamRole.TeamLead);
            if (!isAdmin && team.CreatedById != currentUserId && !isTeamLead)
            {
                return Forbid();
            }

            var member = team.TeamMembers.FirstOrDefault(tm => tm.UserId == userId);
            if (member == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Người dùng không phải là thành viên của đội nhóm", HttpStatusCode.NotFound));
            }

            member.Role = role;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.SuccessResponse("Cập nhật vai trò thành công"));
        }

        /// <summary>
        /// Xóa thành viên khỏi đội nhóm
        /// </summary>
        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(int id, string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Đội nhóm không tồn tại", HttpStatusCode.NotFound));
            }

            // Kiểm tra quyền xóa thành viên
            var isTeamLead = team.TeamMembers.Any(tm => tm.UserId == currentUserId && tm.Role == TeamRole.TeamLead);
            if (!isAdmin && team.CreatedById != currentUserId && !isTeamLead && currentUserId != userId)
            {
                return Forbid();
            }

            var member = team.TeamMembers.FirstOrDefault(tm => tm.UserId == userId);
            if (member == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Người dùng không phải là thành viên của đội nhóm", HttpStatusCode.NotFound));
            }

            // Không thể xóa người tạo đội nhóm
            if (team.CreatedById == userId && !isAdmin)
            {
                return BadRequest(ApiResponse.ErrorResponse("Không thể xóa người tạo đội nhóm", HttpStatusCode.BadRequest));
            }

            _context.TeamMembers.Remove(member);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.SuccessResponse("Xóa thành viên thành công"));
        }

        /// <summary>
        /// Kiểm tra API controller hoạt động
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Teams controller is working");
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.Id == id);
        }
    }
} 