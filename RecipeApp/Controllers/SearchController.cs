// RecipeApp/Controllers/SearchController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeApp.Common.DTOs;
using RecipeApp.Services.Interfaces;

namespace RecipeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ITextAnalysisService _textAnalysisService;
        private readonly IRecipeService _recipeService;

        public SearchController(
            ITextAnalysisService textAnalysisService,
            IRecipeService recipeService)
        {
            _textAnalysisService = textAnalysisService;
            _recipeService = recipeService;
        }

        /// <summary>
        /// מנתח טקסט חופשי ומחזיר ParsedSearchIntent
        /// POST /api/Search/analyze-text
        /// </summary>
        [HttpPost("analyze-text")]
        [AllowAnonymous]
        public async Task<ActionResult<ParsedSearchIntent>> AnalyzeText([FromBody] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest(new { message = "Text cannot be empty." });
            try
            {
                var intent = await _textAnalysisService.AnalyzeAsync(text);
                return Ok(intent);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>
        /// חיפוש מלא — מנתח טקסט ומחזיר מתכונים ישירות
        /// POST /api/Search/advanced
        /// </summary>
        [HttpPost("advanced")]
        [AllowAnonymous]
        public async Task<ActionResult<AdvancedSearchResultDto>> AdvancedSearch([FromBody] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest(new { message = "Text cannot be empty." });
            try
            {
                var intent = await _textAnalysisService.AnalyzeAsync(text);

                // חיפוש לפי תגיות
                var tagResults = intent.Tags.Count > 0
                    ? await _recipeService.SearchByTags(intent.Tags)
                    : new List<RecipeDto>();

                // חיפוש לפי קטגוריה
                var categoryResults = intent.Category != null
                    ? await _recipeService.SearchByCategory(intent.Category)
                    : new List<RecipeDto>();

                // מיזוג תוצאות + פילטור לפי difficulty/prepTime
                var merged = tagResults.Union(categoryResults,
                    EqualityComparer<RecipeDto>.Create(
                        (a, b) => a?.Id == b?.Id,
                        obj => obj?.Id.GetHashCode() ?? 0
                    )).ToList();

                if (intent.DifficultyLevel.HasValue)
                    merged = merged.Where(r => r.Level == intent.DifficultyLevel).ToList();

                if (intent.MaxPrepTime.HasValue)
                    merged = merged.Where(r => r.PrepTime <= intent.MaxPrepTime).ToList();

                return Ok(new AdvancedSearchResultDto
                {
                    Intent = intent,
                    Results = merged
                });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}