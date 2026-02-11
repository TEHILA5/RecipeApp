using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeApp.Common.DTOs;
using RecipeApp.Service.Interfaces;
using RecipeApp.Services.Interfaces;

namespace RecipeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeService _recipeService;
        private readonly IImageService _imageService;

        public RecipeController(IRecipeService recipeService, IImageService imageService)
        {
            _recipeService = recipeService;
            _imageService = imageService;
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

        // POST: api/Recipe - יצירת מתכון עם תמונה
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RecipeDto>> Create([FromForm] RecipeCreateFormDto formDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string? imageUrl = null;

                // אופציה 1: אם יש קובץ תמונה - שמירה בשרת
                if (formDto.ImageFile != null)
                {
                    var fileName = await _imageService.SaveImage(formDto.ImageFile);
                    imageUrl = _imageService.GetImageUrl(fileName);
                }
                // אופציה 2: אם אין קובץ אבל יש URL - שימוש ב-URL
                else if (!string.IsNullOrWhiteSpace(formDto.ImageUrl))
                {
                    imageUrl = formDto.ImageUrl;
                }

                // המרת JSON של מרכיבים למערך
                List<RecipeIngredientCreateDto>? ingredients = null;
                if (!string.IsNullOrWhiteSpace(formDto.IngredientsJson))
                {
                    ingredients = JsonSerializer.Deserialize<List<RecipeIngredientCreateDto>>(
                        formDto.IngredientsJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                }

                // יצירת DTO למתכון
                var createDto = new RecipeCreateDto
                {
                    Name = formDto.Name,
                    Description = formDto.Description,
                    Category = formDto.Category,
                    Instructions = formDto.Instructions,
                    ImageUrl = imageUrl,
                    Servings = formDto.Servings,
                    Level = formDto.Level,
                    PrepTime = formDto.PrepTime,
                    TotalTime = formDto.TotalTime,
                    Ingredients = ingredients
                };

                var created = await _recipeService.CreateRecipe(createDto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
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

        // PATCH: api/Recipe/:id - עדכון מתכון עם תמונה
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RecipeDto>> Update(int id, [FromForm] RecipeUpdateFormDto formDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // קבלת המתכון הקיים
                var existingRecipe = await _recipeService.GetById(id);
                string? imageUrl = existingRecipe.ImageUrl;

                // אופציה 1: אם יש קובץ תמונה חדש
                if (formDto.ImageFile != null)
                {
                    // מחיקת תמונה ישנה (רק אם זו תמונה מקומית ולא URL חיצוני)
                    if (!string.IsNullOrWhiteSpace(imageUrl) &&
                        !imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        var oldFileName = Path.GetFileName(imageUrl);
                        await _imageService.DeleteImage(oldFileName);
                    }

                    // שמירת תמונה חדשה
                    var fileName = await _imageService.SaveImage(formDto.ImageFile);
                    imageUrl = _imageService.GetImageUrl(fileName);
                }
                // אופציה 2: אם יש URL חדש
                else if (!string.IsNullOrWhiteSpace(formDto.ImageUrl))
                {
                    // מחיקת תמונה ישנה (רק אם זו תמונה מקומית)
                    if (!string.IsNullOrWhiteSpace(imageUrl) &&
                        !imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        var oldFileName = Path.GetFileName(imageUrl);
                        await _imageService.DeleteImage(oldFileName);
                    }

                    imageUrl = formDto.ImageUrl;
                }

                // המרת JSON של מרכיבים למערך
                List<RecipeIngredientCreateDto>? ingredients = null;
                if (!string.IsNullOrWhiteSpace(formDto.IngredientsJson))
                {
                    ingredients = JsonSerializer.Deserialize<List<RecipeIngredientCreateDto>>(
                        formDto.IngredientsJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                }

                // יצירת DTO לעדכון
                var recipeDto = new RecipeDto
                {
                    Id = id,
                    Name = formDto.Name ?? existingRecipe.Name,
                    Description = formDto.Description ?? existingRecipe.Description,
                    Category = formDto.Category ?? existingRecipe.Category,
                    Instructions = formDto.Instructions ?? existingRecipe.Instructions,
                    ImageUrl = imageUrl,
                    Servings = formDto.Servings ?? existingRecipe.Servings,
                    Level = formDto.Level ?? existingRecipe.Level,
                    PrepTime = formDto.PrepTime ?? existingRecipe.PrepTime,
                    TotalTime = formDto.TotalTime ?? existingRecipe.TotalTime,
                    Ingredients = ingredients?.Select(i => new RecipeIngredientDto
                    {
                        IngredientId = i.IngredientId,
                        Quantity = i.Quantity,
                        Unit = i.Unit,
                        Importance = i.Importance
                    }).ToList() ?? existingRecipe.Ingredients
                };

                var updated = await _recipeService.UpdateItem(id, recipeDto);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        // DELETE: api/Recipe/:id - מחיקת מתכון כולל התמונה
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var recipe = await _recipeService.GetById(id);

                // מחיקת תמונה (רק אם זו תמונה מקומית ולא URL חיצוני)
                if (!string.IsNullOrWhiteSpace(recipe.ImageUrl) &&
                    !recipe.ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var fileName = Path.GetFileName(recipe.ImageUrl);
                    await _imageService.DeleteImage(fileName);
                }

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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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

        // GET: api/Recipe/recommended
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