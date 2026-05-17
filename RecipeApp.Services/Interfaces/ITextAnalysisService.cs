using RecipeApp.Common.DTOs;

namespace RecipeApp.Services.Interfaces
{
    public interface ITextAnalysisService
    {
        Task<ParsedSearchIntent> AnalyzeAsync(string text, List<string>? knownIngredients = null);
    }
}