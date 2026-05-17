namespace RecipeApp.Common.DTOs
{
    public class ParsedSearchIntent
    {
        public string? Category { get; set; }     
        public List<string> Tags { get; set; } = []; 
        public int? DifficultyLevel { get; set; }     
        public int? MaxPrepTime { get; set; }     
        public List<string> Keywords { get; set; } = [];
        public List<string> IngredientKeywords { get; set; } = [];
        public string OriginalText { get; set; } = ""; 
    }
}