// RecipeApp.Services/Services/TextAnalysisService.cs
using Catalyst;
using FuzzySharp;
using FuzzySharp.SimilarityRatio;
using Microsoft.Extensions.Logging;
using Mosaik.Core;
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

        // ── רשימת קטגוריות ──
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

        // ── רמות קושי ──
        private static readonly Dictionary<int, List<string>> _difficultyKeywords = new()
        {
            { 1, ["easy", "simple", "quick", "light", "basic", "beginner", "fast", "effortless", "no-bake", "nobake"] },
            { 2, ["medium", "moderate", "intermediate", "average"] },
            { 3, ["hard", "difficult", "advanced", "complex", "challenging", "expert", "elaborate"] }
        };

        // ── Stop words ──
        private static readonly HashSet<string> _stopWords =
        [
            "i", "want", "a", "an", "the", "and", "or", "for", "to", "of",
            "me", "my", "make", "bake", "cook", "find", "get", "with",
            "some", "please", "can", "would", "like", "looking", "need",
            "something", "recipe", "recipes", "dessert", "desserts",
            "that", "is", "are", "was", "have", "has", "be", "do",
            "it", "this", "these", "give", "show", "suggest"
        ];

        // ── Category aliases ──
        private static readonly Dictionary<string, string> _categoryAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            { "cake", "Cakes" }, { "cakes", "Cakes" },
            { "cupcake", "Cupcakes" }, { "cupcakes", "Cupcakes" },
            { "cookie", "Cookies" }, { "cookies", "Cookies" }, { "biscuit", "Cookies" },
            { "brownie", "Brownies" }, { "brownies", "Brownies" },
            { "icecream", "IceCream" }, { "ice cream", "IceCream" }, { "ice-cream", "IceCream" }, { "gelato", "IceCream" },
            { "cheesecake", "Cheesecakes" }, { "cheesecakes", "Cheesecakes" }, { "cheese cakes", "Cheesecakes" }, { "cheese cake", "Cheesecakes" },
            { "pie", "Pies" }, { "pies", "Pies" },
            { "tart", "Tarts" }, { "tarts", "Tarts" },
            { "donut", "Donuts" }, { "donuts", "Donuts" }, { "doughnut", "Donuts" },
            { "waffle", "Waffles" }, { "waffles", "Waffles" },
            { "crepe", "Crepes" }, { "crepes", "Crepes" },
            { "truffle", "Truffles" }, { "truffles", "Truffles" },
            { "pastry", "Pastries" }, { "pastries", "Pastries" },
            { "mousse", "Mousse" },
            { "pudding", "Puddings" }, { "puddings", "Puddings" },
            { "tiramisu", "Tiramisu" },
            { "churro", "Churros" }, { "churros", "Churros" },
            { "crumble", "Crumbles" }, { "crumbles", "Crumbles" },
        };

        // ── תגיות תיאוריות ──
        private static readonly HashSet<string> _descriptiveWords =
        [
            "chocolate", "vanilla", "strawberry", "lemon", "caramel", "cinnamon",
            "festive", "light", "heavy", "creamy", "crispy", "fluffy", "moist",
            "frozen", "cold", "warm", "hot", "fresh",
            "sweet", "sour", "bitter", "fruity", "nutty",
            "gluten-free", "vegan", "dairy-free", "sugar-free", "healthy",
            "classic", "traditional", "fancy", "elegant", "rustic",
            "birthday", "holiday", "summer", "winter", "christmas",
            "no-bake", "baked", "fried", "steamed"
        ];

        private Pipeline? _nlpPipeline;
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private bool _initialized = false;
        private bool _catalystAvailable = false;

        // ── אתחול Catalyst — אם נכשל, ממשיך בלעדיו ──
        private async Task EnsureInitializedAsync()
        {
            if (_initialized) return;
            await _initLock.WaitAsync();
            try
            {
                if (_initialized) return;

                try
                {
                    _logger.LogInformation("Initializing Catalyst NLP pipeline...");
                    Storage.Current = new DiskStorage("catalyst-models");
                    _nlpPipeline = await Pipeline.ForAsync(Language.English);
                    _catalystAvailable = true;
                    _logger.LogInformation("Catalyst initialized successfully.");
                }
                catch (Exception ex)
                {
                    // ✅ Catalyst נכשל — נמשיך עם FuzzySharp בלבד
                    _logger.LogWarning("Catalyst initialization failed: {Message}. Falling back to FuzzySharp only.", ex.Message);
                    _catalystAvailable = false;
                    _nlpPipeline = null;
                }

                _initialized = true;
            }
            finally { _initLock.Release(); }
        }

        public async Task<ParsedSearchIntent> AnalyzeAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new ParsedSearchIntent { OriginalText = text ?? "" };

            try
            {
                await EnsureInitializedAsync();

                var intent = new ParsedSearchIntent { OriginalText = text };
                var lower = text.ToLowerInvariant();

                // שלב 1: Tokenize
                var meaningfulTokens = ExtractMeaningfulTokens(text);
                _logger.LogInformation("Tokens extracted: {Tokens}", string.Join(", ", meaningfulTokens));

                // שלב 2: קטגוריה
                intent.Category = DetectCategory(lower, meaningfulTokens);
                _logger.LogInformation("Category detected: {Category}", intent.Category ?? "none");

                // שלב 3: רמת קושי
                intent.DifficultyLevel = DetectDifficulty(lower, meaningfulTokens);

                // שלב 4: זמן הכנה
                intent.MaxPrepTime = DetectPrepTime(lower);

                // שלב 5: Tags + Keywords
                var usedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (intent.Category != null)
                    usedWords.UnionWith(_categoryAliases.Keys);

                intent.Tags = ExtractTags(meaningfulTokens, usedWords);
                intent.Keywords = meaningfulTokens
                    .Where(t => !usedWords.Contains(t) && t.Length > 2)
                    .Distinct()
                    .ToList();

                _logger.LogInformation("Tags: {Tags}, Keywords: {Keywords}",
                    string.Join(", ", intent.Tags),
                    string.Join(", ", intent.Keywords));

                return intent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AnalyzeAsync failed for text: {Text}", text);
                throw;
            }
        }

        // ── Tokenize — Catalyst אם זמין, אחרת Fallback ──
        private List<string> ExtractMeaningfulTokens(string text)
        {
            if (_catalystAvailable && _nlpPipeline != null)
            {
                try
                {
                    var doc = new Document(text, Language.English);
                    _nlpPipeline.ProcessSingle(doc);

                    return doc.Spans
                        .SelectMany(s => s.Tokens)
                        .Where(t => !_stopWords.Contains(t.Value.ToLowerInvariant()))
                        .Where(t => t.Value.Length > 1)
                        .Select(t => t.Value.ToLowerInvariant())
                        .Distinct()
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Catalyst processing failed: {Message}. Using fallback.", ex.Message);
                }
            }

            return FallbackTokenize(text);
        }

        private List<string> FallbackTokenize(string text) =>
            text.ToLowerInvariant()
                .Split([' ', ',', '.', '!', '?', '-', '\''], StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !_stopWords.Contains(w) && w.Length > 1)
                .Distinct()
                .ToList();

        // ── זיהוי קטגוריה ──
        private string? DetectCategory(string lower, List<string> tokens)
        {
            // 1. בדיקה מדויקת
            foreach (var alias in _categoryAliases.OrderByDescending(a => a.Key.Length))
                if (lower.Contains(alias.Key))
                    return alias.Value;

            // 2. FuzzySharp
            string? bestCategory = null;
            int bestScore = 0;

            foreach (var token in tokens)
            {
                var result = Process.ExtractOne(
                    token, _allCategories,
                    scorer: ScorerCache.Get<FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>());

                if (result != null && result.Score > 75 && result.Score > bestScore)
                {
                    bestScore = result.Score;
                    bestCategory = result.Value;
                }
            }

            return bestCategory;
        }

        // ── זיהוי רמת קושי ──
        private int? DetectDifficulty(string lower, List<string> tokens)
        {
            foreach (var (level, keywords) in _difficultyKeywords)
                if (keywords.Any(k => lower.Contains(k) || tokens.Contains(k)))
                    return level;
            return null;
        }

        // ── זיהוי זמן הכנה ──
        private int? DetectPrepTime(string lower)
        {
            var timePatterns = new (string pattern, int minutes)[]
            {
                ("15 min", 15), ("30 min", 30), ("45 min", 45),
                ("1 hour", 60), ("quick", 30), ("fast", 20)
            };

            foreach (var (pattern, minutes) in timePatterns)
                if (lower.Contains(pattern))
                    return minutes;

            return null;
        }

        // ── חילוץ Tags ──
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