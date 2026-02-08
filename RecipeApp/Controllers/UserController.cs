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
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public UserController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        // POST: api/User/register - כולם יכולים
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult> Register([FromBody] UserCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var user = await _userService.Register(createDto);
                var token = GenerateToken(user);
                var userDto = new UserDto
                {
                    Name = user.Name,
                    Phone = user.Phone,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                };
                return Ok(new { user=userDto, token });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // POST: api/User/login - כולם יכולים
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var user = await _userService.Login(loginDto);
                var token = GenerateToken(user); 
                var userDto = new UserDto
                {
                    Name = user.Name,
                    Phone = user.Phone,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                };
                return Ok(new { user=userDto, token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/User/me - משתמש רואה פרטי עצמו
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetMe()
        { 
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userService.GetById(userId);

                var userDto = new UserDto
                {
                    Name = user.Name,
                    Phone = user.Phone,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // PATCH: api/User/me -  למשתמש עדכון עצמי  
        [HttpPatch("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UserUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var userId = GetCurrentUserId();  
                var userToUpdate = new UserAdminDto
                {
                    Id = userId,
                    Name = updateDto.Name,
                    Phone = updateDto.Phone,
                    Email = updateDto.Email
                };

                var updated = await _userService.UpdateItem(userId, userToUpdate);

                var userDto = new UserDto
                {
                    Name = updated.Name,
                    Phone = updated.Phone,
                    Email = updated.Email,
                    CreatedAt = updated.CreatedAt
                };

                return Ok(userDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // DELETE: api/User/me - מחיקת עצמי
        [HttpDelete("me")]
        [Authorize]
        public async Task<IActionResult> DeleteMe()
        { 
            try
            {
                var userId = GetCurrentUserId();
                await _userService.DeleteItem(userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/User - רק מנהל
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserDto>>> GetAll()
        {
            try
            {
                var users = await _userService.GetAll();
                var userAdminDtos = users.Select(u => new UserAdminDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Phone = u.Phone,
                    Email = u.Email,
                    CreatedAt = u.CreatedAt
                }).ToList();

                return Ok(userAdminDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/User/5 -  מנהל
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            try
            {
                var user = await _userService.GetById(id);

                var userAdminDto = new UserAdminDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Phone = user.Phone,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                };

                return Ok(userAdminDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // PATCH: api/User/5 -  מנהל
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserAdminDto>> UpdateUser(int id, [FromBody] UserAdminUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var userToUpdate = new UserAdminDto
                {
                    Id = id,
                    Name = updateDto.Name,
                    Phone = updateDto.Phone,
                    Email = updateDto.Email
                };

                var updated = await _userService.UpdateItem(id, userToUpdate);

                var userAdminDto = new UserAdminDto
                {
                    Id = updated.Id,
                    Name = updated.Name,
                    Phone = updated.Phone,
                    Email = updated.Email,
                    CreatedAt = updated.CreatedAt
                };

                return Ok(userAdminDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // DELETE: api/User/:id -  מנהל
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _userService.DeleteItem(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }


        // Helper Methods 

        private string GenerateToken(UserAdminDto user)
        {
            var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);

            var role = IsAdminEmail(user.Email) ? "Admin" : "User";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int GetCurrentUserId()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var userIdClaim = identity?.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }

        private bool IsAdmin()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value == "Admin";
        }

        private bool IsAdminEmail(string email)
        {
            var adminEmail = _configuration["AdminEmail"] ?? "admin@recipeapp.com";
            return string.Equals(email, adminEmail, StringComparison.OrdinalIgnoreCase);
        }
    }
}

