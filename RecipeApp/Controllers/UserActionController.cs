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

        // POST: api/UserAction/comment - יצירת תגובה
        [HttpPost("comment")]
        public async Task<ActionResult<UserActionDto>> CreateComment([FromBody] CommentCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var userId = GetCurrentUserId();  
                var created = await _userActionService.CreateComment(userId, createDto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // POST: api/UserAction/book - שמירת מתכון
        [HttpPost("book")]
        public async Task<ActionResult<UserActionDto>> CreateBook([FromBody] BookCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var userId = GetCurrentUserId();  
                var created = await _userActionService.CreateBook(userId, createDto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // POST: api/UserAction/history - הוספת חיפוש להיסטוריה
        [HttpPost("history")]
        public async Task<ActionResult<UserActionDto>> CreateHistory([FromBody] HistoryCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var userId = GetCurrentUserId();  
                var created = await _userActionService.CreateHistory(userId, createDto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
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

        // DELETE: api/UserAction/comment/recipe/5 - מחיקת תגובה שלי על מתכון
        [HttpDelete("comment/recipe/{recipeId}")]
        public async Task<IActionResult> DeleteMyComment(int recipeId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var comments = await _userActionService.GetUserComments(userId);
                var comment = comments.FirstOrDefault(c => c.RecipeId == recipeId);

                if (comment == null)
                    return NotFound(new { message = "Comment not found on this recipe." });

                await _userActionService.DeleteItem(comment.Id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // DELETE: api/UserAction/book/recipe/5 - מחיקת שמירה של מתכון
        [HttpDelete("book/recipe/{recipeId}")]
        public async Task<IActionResult> DeleteMyBook(int recipeId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var saved = await _userActionService.GetUserSavedRecipes(userId);
                var book = saved.FirstOrDefault(b => b.RecipeId == recipeId);

                if (book == null)
                    return NotFound(new { message = "Saved recipe not found." });

                await _userActionService.DeleteItem(book.Id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
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

        // GET: api/UserAction/5 - Action ספציפי 
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserActionDto>> GetById(int id)
        {
            try
            {
                var action = await _userActionService.GetById(id);
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

        // GET: api/UserAction/user/5/comments - תגובות של משתמש ספציפי (Admin)
        [HttpGet("user/{userId}/comments")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserActionDto>>> GetUserComments(int userId)
        {
            try
            {
                var comments = await _userActionService.GetUserComments(userId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/UserAction/user/5/history - היסטוריה של משתמש ספציפי (Admin)
        [HttpGet("user/{userId}/history")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserActionDto>>> GetUserHistory(int userId)
        {
            try
            {
                var history = await _userActionService.GetUserHistory(userId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/UserAction/user/5/saved - מתכונים שמורים של משתמש ספציפי (Admin)
        [HttpGet("user/{userId}/saved")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserActionDto>>> GetUserSavedRecipes(int userId)
        {
            try
            {
                var saved = await _userActionService.GetUserSavedRecipes(userId);
                return Ok(saved);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/UserAction/user/5/preferences - העדפות של משתמש ספציפי (Admin)
        [HttpGet("user/{userId}/preferences")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserPreferencesDto>> GetUserPreferences(int userId)
        {
            try
            {
                var preferences = await _userActionService.GetUserPreferences(userId);
                return Ok(preferences);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // DELETE: api/UserAction/admin/5 - מחיקה ע"י Admin
        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAction(int id)
        {
            try
            {
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

        // Helper Methods
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