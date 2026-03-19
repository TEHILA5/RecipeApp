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
        private readonly ITextAnalysisService _textAnalysis;
        private readonly IRecipeService _recipes;

        public SearchController(ITextAnalysisService textAnalysisService, IRecipeService recipeService)
        {
            _textAnalysis = textAnalysisService;
            _recipes = recipeService;
        }

        [HttpPost("analyze-text")]
        [AllowAnonymous]
        public async Task<ActionResult<ParsedSearchIntent>> AnalyzeText([FromBody] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest(new { message = "Text cannot be empty." });
            try
            {
                return Ok(await _textAnalysis.AnalyzeAsync(text));
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("advanced")]
        [AllowAnonymous]
        public async Task<ActionResult<AdvancedSearchResultDto>> AdvancedSearch([FromBody] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest(new { message = "Text cannot be empty." });
            try
            {
                var intent = await _textAnalysis.AnalyzeAsync(text);

                var tagResults = intent.Tags.Count > 0
                    ? await _recipes.SearchByTags(intent.Tags)
                    : new List<RecipeDto>();

                var categoryResults = intent.Category != null
                    ? await _recipes.SearchByCategory(intent.Category)
                    : new List<RecipeDto>();

                var merged = tagResults.Union(categoryResults,
                    EqualityComparer<RecipeDto>.Create(
                        (a, b) => a?.Id == b?.Id,
                        obj => obj?.Id.GetHashCode() ?? 0
                    )).ToList();

                if (intent.DifficultyLevel.HasValue)
                    merged = merged.Where(r => r.Level == intent.DifficultyLevel).ToList();

                if (intent.MaxPrepTime.HasValue)
                    merged = merged.Where(r => r.PrepTime <= intent.MaxPrepTime).ToList();

                return Ok(new AdvancedSearchResultDto { Intent = intent, Results = merged });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}