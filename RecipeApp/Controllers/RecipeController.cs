using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeApp.Common.DTOs;
using RecipeApp.Services.Interfaces;

namespace RecipeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeService _recipeService;

        public RecipeController(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        // GET: api/Recipe
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<RecipeDto>>> GetAll()
        {
            try { return Ok(await _recipeService.GetAll()); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // GET: api/Recipe/:id
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<RecipeDto>> GetById(int id)
        {
            try { return Ok(await _recipeService.GetById(id)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // POST: api/Recipe
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RecipeDto>> Create([FromBody] RecipeCreateDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var created = await _recipeService.CreateRecipe(createDto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // PATCH: api/Recipe/:id
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RecipeDto>> Update(int id, [FromBody] RecipeUpdateDto updateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var recipeDto = new RecipeDto
                {
                    Id = id,
                    Name = updateDto.Name,
                    Description = updateDto.Description,
                    Category = updateDto.Category,
                    Instructions = updateDto.Instructions,
                    ArrImage = updateDto.ArrImage,
                    Servings = updateDto.Servings,
                    Level = updateDto.Level,
                    PrepTime = updateDto.PrepTime,
                    TotalTime = updateDto.TotalTime,
                    Tags = updateDto.Tags, // ✅
                    Ingredients = updateDto.Ingredients?.Select(i => new RecipeIngredientDto
                    {
                        IngredientId = i.IngredientId,
                        Quantity = i.Quantity,
                        Unit = i.Unit,
                        Importance = i.Importance,
                    }).ToList()
                };
                return Ok(await _recipeService.UpdateItem(id, recipeDto));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // DELETE: api/Recipe/:id
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try { await _recipeService.DeleteItem(id); return NoContent(); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // GET: api/Recipe/category/Cakes
        [HttpGet("category/{category}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<RecipeDto>>> SearchByCategory(string category)
        {
            try { return Ok(await _recipeService.SearchByCategory(category)); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // POST: api/Recipe/search-by-ingredients
        [HttpPost("search-by-ingredients")]
        public async Task<ActionResult<List<RecipeDto>>> SearchByIngredients([FromBody] List<string> ingredients)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try { return Ok(await _recipeService.SearchByIngredients(ingredients)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ✅ POST: api/Recipe/search-by-tags 
        [HttpPost("search-by-tags")]
        [AllowAnonymous]
        public async Task<ActionResult<List<RecipeDto>>> SearchByTags([FromBody] List<string> tags)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try { return Ok(await _recipeService.SearchByTags(tags)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // GET: api/Recipe/recommended
        [HttpGet("recommended")]
        public async Task<ActionResult<List<RecipeDto>>> GetRecommended()
        {
            try
            {
                var userId = GetCurrentUserId();
                return Ok(await _recipeService.GetRecommendedForUser(userId));
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        private int GetCurrentUserId()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var userIdClaim = identity?.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }
    }
}