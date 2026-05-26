using RecipeApp.Common.DTOs;
using RecipeApp.Services.Interfaces;

namespace RecipeApp.Services.Services
{
    public class SearchService : ISearchService
    {
        private readonly IRecipeService _recipes;
        private readonly ITextAnalysisService _textAnalysis;

        public SearchService(IRecipeService recipes, ITextAnalysisService textAnalysis)
        {
            _recipes = recipes;
            _textAnalysis = textAnalysis;
        }

        public async Task<AdvancedSearchResultDto> AdvancedSearchAsync(string text)
        {
            var allIngredientNames = await _recipes.GetAllIngredientNames();
            var intent = await _textAnalysis.AnalyzeAsync(text, allIngredientNames);
            var all = await _recipes.GetAll();

            var ranked = all
                .Select(r => RankRecipe(r, intent))
                .Where(r => r.MatchScore > 0)
                .OrderByDescending(r => r.MatchScore)
                .ToList();

            if (ranked.Count == 0)
            {
                ranked = all
                    .OrderByDescending(r => r.AverageRating)
                    .Take(6)
                    .Select(r => new RankedRecipeDto
                    {
                        Recipe = r,
                        MatchScore = 1,
                        MatchLabel = "Suggested for you",
                        MatchedCriteria = [],
                        MissedCriteria = ["No criteria matched"]
                    })
                    .ToList();
            }

            return new AdvancedSearchResultDto { Intent = intent, Results = ranked };
        }

        private RankedRecipeDto RankRecipe(RecipeDto recipe, ParsedSearchIntent intent)
        {
            var matched = new List<string>();
            var missed = new List<string>();

            bool categoryMatch = intent.Category == null ||
                string.Equals(recipe.Category?.ToString(), intent.Category, StringComparison.OrdinalIgnoreCase);
            if (intent.Category != null)
                (categoryMatch ? matched : missed).Add($"Category: {intent.Category}");

            bool diffMatch = !intent.DifficultyLevel.HasValue || recipe.Level == intent.DifficultyLevel;
            if (intent.DifficultyLevel.HasValue)
                (diffMatch ? matched : missed).Add($"Difficulty: {LevelLabel(intent.DifficultyLevel.Value)}");

            bool timeMatch = !intent.MaxPrepTime.HasValue || recipe.PrepTime <= intent.MaxPrepTime;
            if (intent.MaxPrepTime.HasValue)
                (timeMatch ? matched : missed).Add($"Prep time ≤ {intent.MaxPrepTime} min");

            int tagCount = intent.Tags.Count;
            int matchedTags = tagCount > 0 ? CountMatchingTags(recipe, intent.Tags) : 0;
            if (tagCount > 0)
            {
                if (matchedTags > 0)
                    matched.Add($"Tags: {matchedTags}/{tagCount} matched");
                else
                    missed.Add($"Tags: 0/{tagCount} matched");
            }

            int ingredientCount = intent.IngredientKeywords.Count;
            int matchedIngredients = ingredientCount > 0
                ? CountMatchingIngredients(recipe, intent.IngredientKeywords)
                : 0;
            if (ingredientCount > 0)
            {
                if (matchedIngredients > 0)
                    matched.Add($"Ingredients: {matchedIngredients}/{ingredientCount} matched");
                else
                    missed.Add($"Ingredients: 0/{ingredientCount} matched");
            }

            bool hasAnyCriteria = intent.Category != null || intent.DifficultyLevel.HasValue
                                  || intent.MaxPrepTime.HasValue || tagCount > 0;
            if (!hasAnyCriteria) return new RankedRecipeDto { MatchScore = 0 };

            int score = 0;
            if (categoryMatch) score += 40;
            if (diffMatch) score += 15;
            if (timeMatch) score += 10;
            if (tagCount > 0) score += (int)(20.0 * matchedTags / tagCount);
            if (ingredientCount > 0) score += (int)(15.0 * matchedIngredients / ingredientCount);

            if (!categoryMatch && matchedTags == 0) return new RankedRecipeDto { MatchScore = 0 };

            string label = score switch
            {
                >= 95 => "⭐ Perfect Match",
                >= 75 => "✅ Great Match",
                >= 55 => "🟡 Partial Match",
                _ => "🔵 Loose Match"
            };

            if (missed.Count == 1 && missed[0].StartsWith("Difficulty"))
                label = "✅ Matches — different difficulty";
            else if (missed.Count == 1 && missed[0].StartsWith("Prep"))
                label = "✅ Matches — longer prep time";

            return new RankedRecipeDto
            {
                Recipe = recipe,
                MatchScore = score,
                MatchLabel = label,
                MatchedCriteria = matched,
                MissedCriteria = missed
            };
        }

        private static int CountMatchingIngredients(RecipeDto recipe, List<string> keywords)
        {
            if (recipe.Ingredients == null || recipe.Ingredients.Count == 0) return 0;
            return keywords.Count(k =>
                recipe.Ingredients.Any(i =>
                    i.IngredientName.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    k.Contains(i.IngredientName, StringComparison.OrdinalIgnoreCase)));
        }

        private static int CountMatchingTags(RecipeDto recipe, List<string> tags)
        {
            if (recipe.Tags == null || recipe.Tags.Count == 0) return 0;
            return tags.Count(t =>
                recipe.Tags.Any(rt =>
                    rt.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                    t.Contains(rt, StringComparison.OrdinalIgnoreCase)));
        }

        private static string LevelLabel(int level) => level switch
        {
            1 => "Easy",
            2 => "Medium",
            3 => "Hard",
            _ => level.ToString()
        };
    }
}