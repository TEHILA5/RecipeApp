// RecipeApp.Common/DTOs/ParsedSearchIntent.cs
namespace RecipeApp.Common.DTOs
{
    public class ParsedSearchIntent
    {
        public string? Category { get; set; }          // "Cakes", "Cookies"...
        public List<string> Tags { get; set; } = [];   // ["chocolate", "festive", "light"]
        public int? DifficultyLevel { get; set; }      // 1=Easy, 2=Medium, 3=Hard
        public int? MaxPrepTime { get; set; }          // בדקות
        public List<string> Keywords { get; set; } = []; // מילות מפתח נוספות
        public string OriginalText { get; set; } = ""; // הטקסט המקורי
    }
}