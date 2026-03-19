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
        private readonly IUserActionService _actions;

        public UserActionController(IUserActionService userActionService)
        {
            _actions = userActionService;
        }

        [HttpPost("comment")]
        public async Task<ActionResult<UserActionDto>> CreateComment([FromBody] CommentCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var created = await _actions.CreateComment(GetCurrentUserId(), dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("book")]
        public async Task<ActionResult<UserActionDto>> CreateBook([FromBody] BookCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var created = await _actions.CreateBook(GetCurrentUserId(), dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("history")]
        public async Task<ActionResult<UserActionDto>> CreateHistory([FromBody] HistoryCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var created = await _actions.CreateHistory(GetCurrentUserId(), dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("my-history")]
        public async Task<ActionResult<List<UserActionDto>>> GetMyHistory()
        {
            try { return Ok(await _actions.GetUserHistory(GetCurrentUserId())); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpGet("my-saved")]
        public async Task<ActionResult<List<UserActionDto>>> GetMySavedRecipes()
        {
            try { return Ok(await _actions.GetUserSavedRecipes(GetCurrentUserId())); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpGet("my-preferences")]
        public async Task<ActionResult<UserPreferencesDto>> GetMyPreferences()
        {
            try { return Ok(await _actions.GetUserPreferences(GetCurrentUserId())); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpGet("my-comments")]
        public async Task<ActionResult<List<UserActionDto>>> GetMyComments()
        {
            try { return Ok(await _actions.GetUserComments(GetCurrentUserId())); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpDelete("comment/recipe/{recipeId}")]
        public async Task<IActionResult> DeleteMyComment(int recipeId)
        {
            try
            {
                var comments = await _actions.GetUserComments(GetCurrentUserId());
                var comment = comments.FirstOrDefault(c => c.RecipeId == recipeId);
                if (comment == null) return NotFound(new { message = "Comment not found on this recipe." });
                await _actions.DeleteItem(comment.Id);
                return NoContent();
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpDelete("book/recipe/{recipeId}")]
        public async Task<IActionResult> DeleteMyBook(int recipeId)
        {
            try
            {
                var saved = await _actions.GetUserSavedRecipes(GetCurrentUserId());
                var book = saved.FirstOrDefault(b => b.RecipeId == recipeId);
                if (book == null) return NotFound(new { message = "Saved recipe not found." });
                await _actions.DeleteItem(book.Id);
                return NoContent();
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpGet("recipe/{recipeId}/comments")]
        [AllowAnonymous]
        public async Task<ActionResult<List<UserActionDto>>> GetRecipeComments(int recipeId)
        {
            try { return Ok(await _actions.GetRecipeComments(recipeId)); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserActionDto>>> GetAll()
        {
            try { return Ok(await _actions.GetAll()); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserActionDto>> GetById(int id)
        {
            try { return Ok(await _actions.GetById(id)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpGet("user/{userId}/comments")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserActionDto>>> GetUserComments(int userId)
        {
            try { return Ok(await _actions.GetUserComments(userId)); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpGet("user/{userId}/history")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserActionDto>>> GetUserHistory(int userId)
        {
            try { return Ok(await _actions.GetUserHistory(userId)); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpGet("user/{userId}/saved")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserActionDto>>> GetUserSavedRecipes(int userId)
        {
            try { return Ok(await _actions.GetUserSavedRecipes(userId)); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpGet("user/{userId}/preferences")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserPreferencesDto>> GetUserPreferences(int userId)
        {
            try { return Ok(await _actions.GetUserPreferences(userId)); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAction(int id)
        {
            try { await _actions.DeleteItem(id); return NoContent(); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Internal server error", error = ex.Message }); }
        }

        [HttpGet("stats/weekly-categories")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<WeeklyCategoryStatsDto>>> GetWeeklyCategoryStats()
        {
            try { return Ok(await _actions.GetWeeklyCategoryStats()); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        private int GetCurrentUserId()
        {
            var claim = (HttpContext.User.Identity as ClaimsIdentity)?.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(claim?.Value ?? "0");
        }
    }
}