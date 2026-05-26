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
        private readonly ISearchService _search;

        public SearchController(ITextAnalysisService textAnalysisService, ISearchService searchService)
        {
            _textAnalysis = textAnalysisService;
            _search = searchService;
        }

        [HttpPost("analyze-text")]
        public async Task<ActionResult<ParsedSearchIntent>> AnalyzeText([FromBody] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest(new { message = "Text cannot be empty." });
            try { return Ok(await _textAnalysis.AnalyzeAsync(text)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("advanced")]
        public async Task<ActionResult<AdvancedSearchResultDto>> AdvancedSearch([FromBody] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest(new { message = "Text cannot be empty." });
            try { return Ok(await _search.AdvancedSearchAsync(text)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}