using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Models.API;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace ProjectManagementSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Đăng nhập và lấy token JWT
        /// </summary>
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Dữ liệu không hợp lệ", HttpStatusCode.BadRequest, ModelState));
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Không tìm thấy người dùng", HttpStatusCode.NotFound));
            }

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized(ApiResponse.ErrorResponse("Email hoặc mật khẩu không đúng", HttpStatusCode.Unauthorized));
            }

            var token = await CreateToken(user);
            return Ok(ApiResponse.SuccessResponse("Đăng nhập thành công", token));
        }

        /// <summary>
        /// Đăng ký tài khoản mới
        /// </summary>
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Dữ liệu không hợp lệ", HttpStatusCode.BadRequest, ModelState));
            }

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                return Conflict(ApiResponse.ErrorResponse("Email đã tồn tại", HttpStatusCode.Conflict));
            }

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedAt = DateTime.Now,
                SecurityStamp = Guid.NewGuid().ToString(),
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    ApiResponse.ErrorResponse("Lỗi khi tạo người dùng", HttpStatusCode.InternalServerError, errors));
            }

            // Mặc định thêm vai trò User cho người dùng mới
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            await _userManager.AddToRoleAsync(user, "User");

            return Ok(ApiResponse.SuccessResponse("Đăng ký thành công"));
        }

        // Thêm một test endpoint để kiểm tra
        [HttpGet]
        [Route("test")]
        public IActionResult Test()
        {
            return Ok("Auth controller is working");
        }

        private async Task<TokenModel> CreateToken(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var tokenValidityInMinutes = Convert.ToInt32(_configuration["JWT:TokenValidityInMinutes"]);
            var expiration = DateTime.Now.AddMinutes(tokenValidityInMinutes);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: expiration,
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new TokenModel
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }
    }
} 