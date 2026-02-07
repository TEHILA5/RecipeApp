using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RecipeApp.Common.DTOs;
using RecipeApp.Services.Interfaces;

namespace RecipeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class IngredientController : ControllerBase
    {
        private readonly IIngredientService _ingredientService;

        public IngredientController(IIngredientService ingredientService)
        {
            _ingredientService = ingredientService;
        }

        // GET: api/Ingredient
        [HttpGet]
        public async Task<ActionResult<List<IngredientDto>>> GetAll()
        {
            try
            {
                var ingredients = await _ingredientService.GetAll();
                return Ok(ingredients);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Ingredient/:id
        [HttpGet("{id}")]
        public async Task<ActionResult<IngredientDto>> GetById(int id)
        {
            try
            {
                var ingredient = await _ingredientService.GetById(id);
                return Ok(ingredient);
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

        // POST: api/Ingredient - רק Admin
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IngredientDto>> Create([FromBody] IngredientCreateDto createDto)
        {
            try
            {
                var created = await _ingredientService.CreateIngredient(createDto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
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

        // PATCH : api/Ingredient/:id - רק Admin
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IngredientDto>> Update(int id, [FromBody] IngredientDto ingredientDto)
        {
            try
            {
                var updated = await _ingredientService.UpdateItem(id, ingredientDto);
                return Ok(updated);
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

        // DELETE: api/Ingredient/:id - רק Admin
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _ingredientService.DeleteItem(id);
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

        // GET: api/Ingredient/by-name?name=flour
        [HttpGet("by-name")]
        public async Task<ActionResult<IngredientDto>> GetByName([FromQuery] string name)
        {
            try
            {
                var ingredient = await _ingredientService.GetByName(name);
                if (ingredient == null)
                    return NotFound(new { message = $"Ingredient '{name}' not found." });
                return Ok(ingredient);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}
