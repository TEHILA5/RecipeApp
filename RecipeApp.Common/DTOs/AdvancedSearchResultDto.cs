namespace RecipeApp.Common.DTOs
{
    public class AdvancedSearchResultDto
    {
        public ParsedSearchIntent Intent { get; set; } = new();
        public List<RankedRecipeDto> Results { get; set; }
    }

    public class RankedRecipeDto
    {
        public RecipeDto Recipe { get; set; } = new();
        public int MatchScore { get; set; } 
        public string MatchLabel { get; set; } = ""; 
        public List<string> MatchedCriteria { get; set; } = [];
        public List<string> MissedCriteria { get; set; } = [];
    }
}