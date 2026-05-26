using System.Text.RegularExpressions;
using FuzzySharp;
using FuzzySharp.SimilarityRatio;
using Microsoft.Extensions.Logging;
using RecipeApp.Common.DTOs;
using RecipeApp.Services.Interfaces;

namespace RecipeApp.Services.Services
{
    public class TextAnalysisService : ITextAnalysisService
    {
        private readonly ILogger<TextAnalysisService> _logger;

        public TextAnalysisService(ILogger<TextAnalysisService> logger)
        {
            _logger = logger;
        }

        private static readonly List<string> _allCategories =
        [
            "Sweats", "Cakes", "Cupcakes", "Cheesecakes", "BundtCakes",
            "Brownies", "Cookies", "Bars", "IceCream", "Mousse",
            "Puddings", "Panna", "Tiramisu", "FrozenDesserts", "Pies",
            "Tarts", "Crumbles", "FruitSalads", "Pastries", "Donuts",
            "Churros", "Crepes", "Waffles", "NoBakeCakes", "Truffles",
            "EnergyBalls", "SoufleeAndCustard", "MilkDesserts",
            "JellyAndGelatin", "TraditionalDesserts"
        ];

        private static readonly Dictionary<int, List<string>> _difficultyKeywords = new()
        {
            { 1, ["easy", "simple", "quick", "light", "basic", "beginner", "fast", "effortless", "no-bake", "nobake"] },
            { 2, ["medium", "moderate", "intermediate", "average"] },
            { 3, ["hard", "difficult", "advanced", "complex", "challenging", "expert", "elaborate"] }
        };

        private static readonly List<string> _negatedDifficulty =
        [
            "not too hard", "not hard", "not too difficult", "not difficult",
            "not too complex", "not complicated", "not challenging",
            "not too easy", "not easy", "not too simple"
        ];

        private static readonly HashSet<string> _stopWords =
        [
            "i", "want", "a", "an", "the", "and", "or", "for", "to", "of",
            "me", "my", "make", "bake", "cook", "find", "get", "with",
            "some", "please", "can", "would", "like", "looking", "need",
            "something", "recipe", "recipes", "dessert", "desserts",
            "that", "is", "are", "was", "have", "has", "be", "do",
            "it", "this", "these", "give", "show", "suggest",
            "not", "too", "very", "really", "quite", "rather",
        ];

        private static readonly Dictionary<string, string> _categoryAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            { "cake", "Cakes" }, { "cakes", "Cakes" },
            { "cupcake", "Cupcakes" }, { "cupcakes", "Cupcakes" },
            { "cookie", "Cookies" }, { "cookies", "Cookies" }, { "biscuit", "Cookies" },
            { "brownie", "Brownies" }, { "brownies", "Brownies" },
            { "icecream", "IceCream" }, { "ice cream", "IceCream" }, { "ice-cream", "IceCream" }, { "gelato", "IceCream" },
            { "cheesecake", "Cheesecakes" }, { "cheesecakes", "Cheesecakes" }, { "cheese cake", "Cheesecakes" },
            { "pie", "Pies" }, { "pies", "Pies" },
            { "tart", "Tarts" }, { "tarts", "Tarts" },
            { "donut", "Donuts" }, { "donuts", "Donuts" }, { "doughnut", "Donuts" },
            { "waffle", "Waffles" }, { "waffles", "Waffles" },
            { "crepe", "Crepes" }, { "crepes", "Crepes" }, { "pancake", "Crepes" },
            { "truffle", "Truffles" }, { "truffles", "Truffles" },
            { "pastry", "Pastries" }, { "pastries", "Pastries" },
            { "mousse", "Mousse" },
            { "pudding", "Puddings" }, { "puddings", "Puddings" },
            { "tiramisu", "Tiramisu" },
            { "churro", "Churros" }, { "churros", "Churros" },
            { "crumble", "Crumbles" }, { "crumbles", "Crumbles" },
            { "muffin", "Cupcakes" },
            { "bar", "Bars" }, { "bars", "Bars" },
            { "energy ball", "EnergyBalls" }, { "bliss ball", "EnergyBalls" },
            { "jelly", "JellyAndGelatin" }, { "gelatin", "JellyAndGelatin" },
            { "panna cotta", "Panna" },
            { "souffle", "SoufleeAndCustard" }, { "soufflé", "SoufleeAndCustard" },
            { "sorbet", "FrozenDesserts" },
            { "fruit salad", "FruitSalads" },
        };

        private static readonly HashSet<string> _descriptiveWords =
        [
            "chocolate", "vanilla", "strawberry", "lemon", "caramel", "cinnamon",
            "coconut", "almond", "peanut", "banana", "apple", "blueberry",
            "raspberry", "mango", "orange", "coffee", "matcha", "pistachio",
            "festive", "light", "heavy", "creamy", "crispy", "fluffy", "moist",
            "cold", "warm", "hot", "fresh", "crunchy", "chewy", "airy", "dense",
            "sweet", "sour", "bitter", "fruity", "nutty", "spicy", "tangy",
            "gluten-free", "vegan", "dairy-free", "sugar-free", "healthy", "no-bake",
            "classic", "traditional", "fancy", "elegant", "rustic", "homemade",
            "birthday", "holiday", "summer", "winter", "breakfast",
            "brunch", "snack", "quick", "kids", "comforting",
            "baked", "fried", "steamed", "frozen",
            "british", "french", "italian", "american", "australian",
            "norwegian", "canadian", "japanese", "moroccan", "tunisian",
            "argentinian", "uruguayan", "jamaican", "ukrainian", "dutch",
            "danish", "scottish", "irish", "spanish", "greek", "turkish",
            "indian", "chinese", "thai", "mexican", "portuguese", "belgian",
        ];

        public Task<ParsedSearchIntent> AnalyzeAsync(string text, List<string>? knownIngredients = null)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Task.FromResult(new ParsedSearchIntent { OriginalText = text ?? "" });

            try
            {
                var intent = new ParsedSearchIntent { OriginalText = text };
                var lower = text.ToLowerInvariant();
                var tokens = Tokenize(text);

                intent.Category = DetectCategory(lower, tokens);
                intent.DifficultyLevel = DetectDifficulty(lower, tokens);
                intent.MaxPrepTime = DetectPrepTime(lower);

                var usedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (intent.Category != null)
                    usedWords.UnionWith(_categoryAliases.Keys);

                intent.Tags = ExtractTags(tokens, usedWords);
                intent.Keywords = tokens
                    .Where(t => !usedWords.Contains(t) && t.Length > 2)
                    .Distinct()
                    .ToList();

                if (knownIngredients != null && knownIngredients.Count > 0)
                    intent.IngredientKeywords = ExtractIngredientMatches(tokens, lower, knownIngredients);

                _logger.LogInformation("Category: {C} | Tags: {T} | Ingredients: {I}",
                    intent.Category ?? "none",
                    string.Join(", ", intent.Tags),
                    string.Join(", ", intent.IngredientKeywords));

                return Task.FromResult(intent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AnalyzeAsync failed for: {Text}", text);
                throw;
            }
        }

        private List<string> ExtractIngredientMatches(List<string> tokens, string lower, List<string> knownIngredients)
        {
            var matched = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var ingredient in knownIngredients)
            {
                var ingredientLower = ingredient.ToLowerInvariant();
                 
                if (lower.Contains(ingredientLower))
                {
                    matched.Add(ingredient);
                    continue;
                }

                foreach (var token in tokens)
                {
                    if (token.Length < 4) continue;

                    var result = Process.ExtractOne(
                        token, [ingredientLower],
                        scorer: ScorerCache.Get<FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>());

                    int threshold = token.Length >= 6 ? 88 : 95;

                    if (result != null && result.Score >= threshold)
                    {
                        matched.Add(ingredient);
                        break;
                    }
                }
            }

            return matched.ToList();
        }

        private List<string> Tokenize(string text) =>
            text.ToLowerInvariant()
                .Split([' ', ',', '.', '!', '?', '-', '\''], StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !_stopWords.Contains(w) && w.Length > 1)
                .Distinct()
                .ToList();

        private string? DetectCategory(string lower, List<string> tokens)
        {
            foreach (var alias in _categoryAliases.OrderByDescending(a => a.Key.Length))
                if (lower.Contains(alias.Key))
                    return alias.Value;

            string? best = null;
            int bestScore = 0;
            foreach (var token in tokens)
            {
                var result = Process.ExtractOne(
                    token, _allCategories,
                    scorer: ScorerCache.Get<FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>());
                if (result != null && result.Score > 95 && result.Score > bestScore)
                {
                    bestScore = result.Score;
                    best = result.Value;
                }
            }
            return best;
        }

        private int? DetectDifficulty(string lower, List<string> tokens)
        {
            if (_negatedDifficulty.Any(n => lower.Contains(n)))
                return null;

            foreach (var (level, keywords) in _difficultyKeywords)
                if (keywords.Any(k => lower.Contains(k) || tokens.Contains(k)))
                    return level;

            return null;
        }

        private int? DetectPrepTime(string lower)
        { 
            var match = Regex.Match(lower,
                @"(\d+)\s*(min|minute|minutes|hour|hours)");

            if (match.Success)
            {
                var value = int.Parse(match.Groups[1].Value);
                var unit = match.Groups[2].Value;
                return unit.StartsWith("hour") ? value * 60 : value;
            }
             
            if (lower.Contains("quick") || lower.Contains("fast")) return 20;

            return null;
        }

        private List<string> ExtractTags(List<string> tokens, HashSet<string> usedWords)
        {
            var tags = new List<string>();
            foreach (var token in tokens)
            {
                if (usedWords.Contains(token)) continue;
                if (_descriptiveWords.Contains(token))
                {
                    tags.Add(token);
                    usedWords.Add(token);
                    continue;
                }
                var match = Process.ExtractOne(
                    token, _descriptiveWords,
                    scorer: ScorerCache.Get<FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>());
                if (match != null && match.Score > 80)
                {
                    tags.Add(match.Value);
                    usedWords.Add(token);
                }
            }
            return tags.Distinct().ToList();
        }
    }
}