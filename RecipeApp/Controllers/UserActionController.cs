using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeApp.Common.DTOs;
using RecipeApp.Services.Interfaces;
using System.Security.Claims;

namespace RecipeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserActionController : ControllerBase
    {
        private readonly IUserActionService _userActionService;

        public UserActionController(IUserActionService userActionService)
        {
            _userActionService = userActionService;
        }

        // GET: api/UserAction - רק Admin רואה הכל
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserActionDto>>> GetAll()
        {
            try
            {
                var actions = await _userActionService.GetAll();
                return Ok(actions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // POST: api/UserAction - משתמש יכול ליצור Action משלו
        [HttpPost]
        public async Task<ActionResult<UserActionDto>> Create([FromBody] UserActionDto actionDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // משתמש יכול ליצור רק Actions עבור עצמו
                if (actionDto.UserId != currentUserId && !IsAdmin())
                    return Forbid();

                var created = await _userActionService.AddItem(actionDto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/UserAction/:id
        [HttpGet("{id}")]
        public async Task<ActionResult<UserActionDto>> GetById(int id)
        {
            try
            {
                var action = await _userActionService.GetById(id);

                // משתמש יכול לראות רק Actions שלו
                var currentUserId = GetCurrentUserId();
                if (action.UserId != currentUserId && !IsAdmin())
                    return Forbid();

                return Ok(action);
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

        // GET: api/UserAction/my-comments - התגובות שלי
        [HttpGet("my-comments")]
        public async Task<ActionResult<List<UserActionDto>>> GetMyComments()
        {
            try
            {
                var userId = GetCurrentUserId();
                var comments = await _userActionService.GetUserComments(userId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/UserAction/my-history - ההיסטוריה שלי
        [HttpGet("my-history")]
        public async Task<ActionResult<List<UserActionDto>>> GetMyHistory()
        {
            try
            {
                var userId = GetCurrentUserId();
                var history = await _userActionService.GetUserHistory(userId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/UserAction/my-saved - המתכונים השמורים שלי
        [HttpGet("my-saved")]
        public async Task<ActionResult<List<UserActionDto>>> GetMySavedRecipes()
        {
            try
            {
                var userId = GetCurrentUserId();
                var saved = await _userActionService.GetUserSavedRecipes(userId);
                return Ok(saved);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/UserAction/my-preferences - ההעדפות שלי
        [HttpGet("my-preferences")]
        public async Task<ActionResult<UserPreferencesDto>> GetMyPreferences()
        {
            try
            {
                var userId = GetCurrentUserId();
                var preferences = await _userActionService.GetUserPreferences(userId);
                return Ok(preferences);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // DELETE: api/UserAction/:id
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var action = await _userActionService.GetById(id);
                var currentUserId = GetCurrentUserId();

                // משתמש יכול למחוק רק Actions שלו
                if (action.UserId != currentUserId && !IsAdmin())
                    return Forbid();

                await _userActionService.DeleteItem(id);
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
    }
}