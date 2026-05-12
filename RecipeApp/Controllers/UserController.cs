using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RecipeApp.Common.DTOs;
using RecipeApp.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RecipeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _users;
        private readonly IConfiguration _config;

        public UserController(IUserService userService, IConfiguration configuration)
        {
            _users = userService;
            _config = configuration;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult> Register([FromBody] UserCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var user = await _users.Register(dto);
                return Ok(new { user = ToUserDto(user), token = GenerateToken(user) });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody] UserLoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var user = await _users.Login(dto);
                return Ok(new { user = ToUserDto(user), token = GenerateToken(user) });
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                await _users.ResetPassword(dto);
                return Ok(new { message = "Password updated successfully." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetMe()
        {
            try
            {
                var user = await _users.GetById(GetCurrentUserId());
                return Ok(ToUserDto(user));
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpPatch("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UserUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var userId = GetCurrentUserId();
                var updated = await _users.UpdateItem(userId, new UserAdminDto
                {
                    Id = userId,
                    Name = dto.Name,
                    Phone = dto.Phone,
                    Email = dto.Email
                });
                return Ok(ToUserDto(updated));
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpDelete("me")]
        [Authorize]
        public async Task<IActionResult> DeleteMe()
        {
            try { await _users.DeleteItem(GetCurrentUserId()); return NoContent(); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserDto>>> GetAll()
        {
            try
            {
                var users = await _users.GetAll();
                return Ok(users.Select(ToAdminDto).ToList());
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            try { return Ok(ToAdminDto(await _users.GetById(id))); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserAdminDto>> UpdateUser(int id, [FromBody] UserAdminUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var updated = await _users.UpdateItem(id, new UserAdminDto
                {
                    Id = id,
                    Name = dto.Name,
                    Phone = dto.Phone,
                    Email = dto.Email
                });
                return Ok(ToAdminDto(updated));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try { await _users.DeleteItem(id); return NoContent(); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        private string GenerateToken(UserAdminDto user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var role = IsAdminEmail(user.Email) ? "Admin" : "User";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, role)
            };

            var expirationMinutes = _config.GetValue<int>("Jwt:ExpirationMinutes");
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(expirationMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int GetCurrentUserId()
        {
            var claim = (HttpContext.User.Identity as ClaimsIdentity)?.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(claim?.Value ?? "0");
        }

        private bool IsAdminEmail(string email)
        {
            var adminEmail = _config["AdminEmail"] ?? "admin@recipeapp.com";
            return string.Equals(email, adminEmail, StringComparison.OrdinalIgnoreCase);
        }

        private static UserDto ToUserDto(UserAdminDto u) => new()
        {
            Name = u.Name,
            Phone = u.Phone,
            Email = u.Email,
            CreatedAt = u.CreatedAt
        };

        private static UserAdminDto ToAdminDto(UserAdminDto u) => new()
        {
            Id = u.Id,
            Name = u.Name,
            Phone = u.Phone,
            Email = u.Email,
            CreatedAt = u.CreatedAt
        };
    }
}