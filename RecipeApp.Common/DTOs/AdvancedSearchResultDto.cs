namespace RecipeApp.Common.DTOs
{
    public class AdvancedSearchResultDto
    {
        public ParsedSearchIntent Intent { get; set; } = new();
        public List<RecipeDto> Results { get; set; } = [];
    }
}