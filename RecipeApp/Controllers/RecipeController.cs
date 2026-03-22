using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeApp.Common.DTOs;
using RecipeApp.Services.Interfaces;
using RecipeApp.Repository.Entities;

namespace RecipeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeService _recipes;

        public RecipeController(IRecipeService recipeService)
        {
            _recipes = recipeService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<RecipeDto>>> GetAll()
        {
            try { return Ok(await _recipes.GetAll()); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<RecipeDto>> GetById(int id)
        {
            try { return Ok(await _recipes.GetById(id)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RecipeDto>> Create([FromBody] RecipeCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var created = await _recipes.CreateRecipe(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RecipeDto>> Update(int id, [FromBody] RecipeUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var createDto = new RecipeCreateDto
                {
                    Name = dto.Name ?? "",
                    Description = dto.Description ?? "",
                    Category = dto.Category ?? RecipeCategory.Sweats,
                    Instructions = dto.Instructions ?? "",
                    ArrImage = dto.ArrImage ?? "",
                    Servings = dto.Servings ?? 4,
                    Level = dto.Level ?? 1,
                    PrepTime = dto.PrepTime ?? 15,
                    TotalTime = dto.TotalTime ?? 60,
                    Tags = dto.Tags,
                    Ingredients = dto.Ingredients?.Select(i => new RecipeIngredientCreateDto
                    {
                        IngredientId = i.IngredientId,
                        Quantity = i.Quantity,
                        Unit = i.Unit ?? "g",
                        Importance = i.Importance,
                    }).ToList() ?? new List<RecipeIngredientCreateDto>()
                };
                return Ok(await _recipes.UpdateRecipe(id, createDto));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try { await _recipes.DeleteItem(id); return NoContent(); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("category/{category}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<RecipeDto>>> SearchByCategory(string category)
        {
            try { return Ok(await _recipes.SearchByCategory(category)); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("search-by-ingredients")]
        public async Task<ActionResult<List<RecipeDto>>> SearchByIngredients([FromBody] List<string> ingredients)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try { return Ok(await _recipes.SearchByIngredients(ingredients)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("search-by-tags")]
        [AllowAnonymous]
        public async Task<ActionResult<List<RecipeDto>>> SearchByTags([FromBody] List<string> tags)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try { return Ok(await _recipes.SearchByTags(tags)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("recommended")]
        public async Task<ActionResult<List<RecipeDto>>> GetRecommended()
        {
            try { return Ok(await _recipes.GetRecommendedForUser(GetCurrentUserId())); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        private int GetCurrentUserId()
        {
            var claim = (HttpContext.User.Identity as ClaimsIdentity)?.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(claim?.Value ?? "0");
        }
    }
}