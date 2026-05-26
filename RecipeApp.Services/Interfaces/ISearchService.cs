using RecipeApp.Common.DTOs;

namespace RecipeApp.Services.Interfaces
{
    public interface ISearchService
    {
        Task<AdvancedSearchResultDto> AdvancedSearchAsync(string text);
    }
}