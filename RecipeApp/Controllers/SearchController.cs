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

                // step 1: get pool by category (or all if no category detected)
                var pool = intent.Category != null
                    ? await _recipes.SearchByCategory(intent.Category)
                    : await _recipes.GetAll();

                // step 2: filter by difficulty if requested
                if (intent.DifficultyLevel.HasValue)
                    pool = pool.Where(r => r.Level == intent.DifficultyLevel).ToList();

                // step 3: filter by prep time if requested
                if (intent.MaxPrepTime.HasValue)
                    pool = pool.Where(r => r.PrepTime <= intent.MaxPrepTime).ToList();

                // step 4: cascade tag filtering
                // try all tags → if no results, try X-1 → X-2 ... → 0 (no tag filter)
                var results = pool;

                if (intent.Tags.Count > 0)
                {
                    for (int required = intent.Tags.Count; required >= 1; required--)
                    {
                        var filtered = pool.Where(r =>
                            CountMatchingTags(r, intent.Tags) >= required
                        ).ToList();

                        if (filtered.Count > 0)
                        {
                            // sort by how many tags match (best first)
                            results = filtered
                                .OrderByDescending(r => CountMatchingTags(r, intent.Tags))
                                .ToList();
                            break;
                        }

                        // if even 1 tag didn't match — return unfiltered pool
                        if (required == 1)
                            results = pool;
                    }
                }

                return Ok(new AdvancedSearchResultDto { Intent = intent, Results = results });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        private static int CountMatchingTags(RecipeDto recipe, List<string> tags)
        {
            if (recipe.Tags == null || recipe.Tags.Count == 0) return 0;
            return tags.Count(t =>
                recipe.Tags.Any(rt =>
                    rt.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                    t.Contains(rt, StringComparison.OrdinalIgnoreCase)));
        }
    }
}