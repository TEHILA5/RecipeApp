using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeApp.Common.DTOs;
using RecipeApp.Services.Interfaces;
using System.Security.Claims;

namespace RecipeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // צריך לכולם Token
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeService _recipeService;

        public RecipeController(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        // GET: api/Recipe
        [HttpGet]
        public async Task<ActionResult<List<RecipeDto>>> GetAll()
        {
            try
            {
                var recipes = await _recipeService.GetAll();
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Recipe/:id
        [HttpGet("{id}")]
        public async Task<ActionResult<RecipeDto>> GetById(int id)
        {
            try
            {
                var recipe = await _recipeService.GetById(id);
                return Ok(recipe);
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

        // POST: api/Recipe - רק מנהל יכול ליצור מתכונים
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RecipeDto>> Create([FromBody] RecipeDto recipeDto)
        {
            try
            {
                var created = await _recipeService.AddItem(recipeDto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // PUT: api/Recipe/:id - רק מנהל יכול לערוך
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RecipeDto>> Update(int id, [FromBody] RecipeDto recipeDto)
        {
            try
            {
                var updated = await _recipeService.UpdateItem(id, recipeDto);
                return Ok(updated);
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

        // DELETE: api/Recipe/:id - רק מנהל יכול למחוק
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _recipeService.DeleteItem(id);
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

        // GET: api/Recipe/category/Cakes
        [HttpGet("category/{category}")]
        public async Task<ActionResult<List<RecipeDto>>> SearchByCategory(string category)
        {
            try
            {
                var recipes = await _recipeService.SearchByCategory(category);
                return Ok(recipes);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // POST: api/Recipe/search-by-ingredients
        [HttpPost("search-by-ingredients")]
        public async Task<ActionResult<List<RecipeDto>>> SearchByIngredients([FromBody] List<string> ingredients)
        {
            try
            {
                var recipes = await _recipeService.SearchByIngredients(ingredients);
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Recipe/recommended - המלצות למשתמש המחובר
        [HttpGet("recommended")]
        public async Task<ActionResult<List<RecipeDto>>> GetRecommended()
        {
            try
            {
                var userId = GetCurrentUserId();
                var recipes = await _recipeService.GetRecommendedForUser(userId);
                return Ok(recipes);
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
    }
}