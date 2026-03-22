using AutoMapper;
using RecipeApp.Common.DTOs;
using RecipeApp.Repository.Entities;
using RecipeApp.Repository.Interfaces;
using RecipeApp.Services.Interfaces;
using System.Text.Json;

namespace RecipeApp.Services.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly IRepository<Recipe> _recipeRepository;
        private readonly IRepository<Ingredient> _ingredientRepository;
        private readonly IRepository<UserAction> _userActionRepository;
        private readonly IMapper _mapper;

        public RecipeService(
            IRepository<Recipe> recipeRepository,
            IRepository<Ingredient> ingredientRepository,
            IRepository<UserAction> userActionRepository,
            IMapper mapper)
        {
            _recipeRepository = recipeRepository;
            _ingredientRepository = ingredientRepository;
            _userActionRepository = userActionRepository;
            _mapper = mapper;
        }

        public async Task<List<RecipeDto>> GetAll()
        {
            var recipes = await _recipeRepository.GetAll();
            var allActions = await _userActionRepository.GetAll();
            return recipes.Select(r => MapRecipeWithStats(r, allActions)).ToList();
        }

        public async Task<RecipeDto> GetById(int id)
        {
            var recipe = await _recipeRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Recipe with id {id} not found.");
            var allActions = await _userActionRepository.GetAll();
            return MapRecipeWithStats(recipe, allActions);
        }

        public async Task<RecipeDto> AddItem(RecipeDto item)
        {
            var recipe = _mapper.Map<Recipe>(item);
            var created = await _recipeRepository.AddItem(recipe);
            var allActions = await _userActionRepository.GetAll();
            return MapRecipeWithStats(created, allActions);
        }

        public async Task<RecipeDto> UpdateItem(int id, RecipeDto item)
        {
            var existing = await _recipeRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Recipe with id {id} not found.");

            if (item.Name != null) existing.Name = item.Name;
            if (item.Description != null) existing.Description = item.Description;
            if (item.Category.HasValue) existing.Category = item.Category.Value;
            if (item.Instructions != null) existing.Instructions = item.Instructions;
            if (item.ArrImage?.Length > 0) existing.ImageUrl = item.ArrImage;
            if (item.Servings.HasValue) existing.Servings = item.Servings.Value;
            if (item.Level.HasValue) existing.Level = item.Level.Value;
            if (item.PrepTime.HasValue) existing.PrepTime = item.PrepTime.Value;
            if (item.TotalTime.HasValue) existing.TotalTime = item.TotalTime.Value;
            if (item.Tags != null) existing.Tags = SerializeTags(item.Tags);

            if (item.Ingredients != null)
            {
                existing.RecipeIngredients = item.Ingredients.Select(ri => new RecipeIngredient
                {
                    IngredientId = ri.IngredientId,
                    Quantity = ri.Quantity,
                    Unit = ri.Unit,
                    Importance = ri.Importance ?? IngredientImportance.Essential
                }).ToList();
            }

            var updated = await _recipeRepository.UpdateItem(id, existing);
            var allActions = await _userActionRepository.GetAll();
            return MapRecipeWithStats(updated, allActions);
        }

        public async Task DeleteItem(int id)
        {
            _ = await _recipeRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Recipe with id {id} not found.");
            await _recipeRepository.DeleteItem(id);
        }

        public async Task<RecipeDto> CreateRecipe(RecipeCreateDto dto)
        {
            var recipe = new Recipe
            {
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                Instructions = dto.Instructions,
                ImageUrl = dto.ArrImage,
                Servings = dto.Servings,
                Level = dto.Level,
                PrepTime = dto.PrepTime,
                TotalTime = dto.TotalTime,
                Tags = SerializeTags(dto.Tags),
                RecipeIngredients = dto.Ingredients?.Select(i => new RecipeIngredient
                {
                    IngredientId = i.IngredientId,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Importance = i.Importance
                }).ToList() ?? new List<RecipeIngredient>()
            };

            var created = await _recipeRepository.AddItem(recipe);
            var loaded = await _recipeRepository.GetById(created.Id);
            var allActions = await _userActionRepository.GetAll();
            return MapRecipeWithStats(loaded, allActions);
        }

        public async Task<RecipeDto> UpdateRecipe(int id, RecipeCreateDto dto)
        {
            // לא קוראים GetById כאן - Repository יטפל בהכל ישירות מה-DB
            var recipe = new Recipe
            {
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                Instructions = dto.Instructions,
                ImageUrl = dto.ArrImage,
                Servings = dto.Servings,
                Level = dto.Level,
                PrepTime = dto.PrepTime,
                TotalTime = dto.TotalTime,
                Tags = SerializeTags(dto.Tags),
                RecipeIngredients = dto.Ingredients?.Select(i => new RecipeIngredient
                {
                    IngredientId = i.IngredientId,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Importance = i.Importance
                }).ToList() ?? new List<RecipeIngredient>()
            };

            var updated = await _recipeRepository.UpdateItem(id, recipe);
            if (updated == null) throw new KeyNotFoundException($"Recipe with id {id} not found.");

            var allActions = await _userActionRepository.GetAll();
            return MapRecipeWithStats(updated, allActions);
        }

        public async Task<List<RecipeDto>> SearchByCategory(string category)
        {
            if (!Enum.TryParse<RecipeCategory>(category, ignoreCase: true, out var categoryEnum))
                throw new ArgumentException($"Invalid category: '{category}'.");

            var recipes = await _recipeRepository.GetAll();
            var allActions = await _userActionRepository.GetAll();

            return recipes
                .Where(r => r.Category == categoryEnum)
                .Select(r => MapRecipeWithStats(r, allActions))
                .ToList();
        }

        public async Task<List<RecipeDto>> SearchByIngredients(List<string> ingredients)
        {
            var allIngredients = await _ingredientRepository.GetAll();

            var ingredientIds = allIngredients
                .Where(i => ingredients.Any(name =>
                    string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase)))
                .Select(i => i.Id)
                .ToHashSet();

            if (ingredientIds.Count == 0) return new List<RecipeDto>();

            var recipes = await _recipeRepository.GetAll();
            var allActions = await _userActionRepository.GetAll();

            return recipes
                .Where(r => r.RecipeIngredients != null && r.RecipeIngredients.Any() && (
                    ingredientIds.All(id => r.RecipeIngredients.Any(ri => ri.IngredientId == id))
                    ||
                    r.RecipeIngredients.All(ri => ingredientIds.Contains(ri.IngredientId))
                ))
                .Select(r => MapRecipeWithStats(r, allActions))
                .ToList();
        }

        public async Task<List<RecipeDto>> SearchByTags(List<string> tags)
        {
            if (tags == null || tags.Count == 0) return new List<RecipeDto>();

            var recipes = await _recipeRepository.GetAll();
            var allActions = await _userActionRepository.GetAll();

            var normalizedTags = tags.Select(t => t.Trim().ToLowerInvariant()).ToList();

            return recipes
                .Where(r =>
                {
                    if (string.IsNullOrEmpty(r.Tags)) return false;
                    var recipeTags = DeserializeTags(r.Tags);
                    return recipeTags.Any(rt =>
                        normalizedTags.Any(t =>
                            rt.ToLowerInvariant().Contains(t) ||
                            t.Contains(rt.ToLowerInvariant())));
                })
                .Select(r => MapRecipeWithStats(r, allActions))
                .ToList();
        }

        public async Task<List<RecipeDto>> GetRecommendedForUser(int userId)
        {
            var allActions = await _userActionRepository.GetAll();
            var myActions = allActions.Where(ua => ua.UserId == userId).ToList();
            var recipes = await _recipeRepository.GetAll();

            const int MIN_RESULTS = 3;

            if (myActions.Count == 0)
            {
                return recipes
                    .OrderByDescending(r => allActions
                        .Where(ua => ua.RecipeId == r.Id && ua.ActionType == UserActionType.Comment && ua.Rating.HasValue)
                        .Select(ua => (double)ua.Rating!.Value)
                        .DefaultIfEmpty(0).Average())
                    .Take(MIN_RESULTS)
                    .Select(r => MapRecipeWithStats(r, allActions))
                    .ToList();
            }

            var recipeDictionary = recipes.ToDictionary(r => r.Id);

            var seenRecipeIds = myActions
                .Where(ua => ua.RecipeId.HasValue)
                .Select(ua => ua.RecipeId!.Value)
                .ToHashSet();

            var mostCommonCategory = myActions
                .Where(ua =>
                    ua.ActionType == UserActionType.History &&
                    ua.RecipeId.HasValue &&
                    recipeDictionary.ContainsKey(ua.RecipeId!.Value))
                .Select(ua => recipeDictionary[ua.RecipeId!.Value].Category)
                .GroupBy(c => c)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Cast<RecipeCategory?>()
                .FirstOrDefault();

            var recommended = new List<Recipe>();

            if (mostCommonCategory.HasValue)
            {
                recommended = recipes
                    .Where(r => r.Category == mostCommonCategory.Value && !seenRecipeIds.Contains(r.Id))
                    .ToList();
            }

            if (recommended.Count < MIN_RESULTS)
            {
                var more = recipes
                    .Where(r => !seenRecipeIds.Contains(r.Id) && !recommended.Any(rec => rec.Id == r.Id))
                    .OrderByDescending(r => allActions
                        .Where(ua => ua.RecipeId == r.Id && ua.ActionType == UserActionType.Comment && ua.Rating.HasValue)
                        .Select(ua => (double)ua.Rating!.Value)
                        .DefaultIfEmpty(0).Average())
                    .Take(MIN_RESULTS - recommended.Count)
                    .ToList();
                recommended.AddRange(more);
            }

            if (recommended.Count < MIN_RESULTS)
            {
                var topRated = recipes
                    .Where(r => !recommended.Any(rec => rec.Id == r.Id))
                    .OrderByDescending(r => allActions
                        .Where(ua => ua.RecipeId == r.Id && ua.ActionType == UserActionType.Comment && ua.Rating.HasValue)
                        .Select(ua => (double)ua.Rating!.Value)
                        .DefaultIfEmpty(0).Average())
                    .Take(MIN_RESULTS - recommended.Count)
                    .ToList();
                recommended.AddRange(topRated);
            }

            return recommended
                .Take(MIN_RESULTS)
                .Select(r => MapRecipeWithStats(r, allActions))
                .ToList();
        }

        private RecipeDto MapRecipeWithStats(Recipe recipe, List<UserAction> allActions)
        {
            var dto = _mapper.Map<RecipeDto>(recipe);

            if (!string.IsNullOrEmpty(recipe.ImageUrl))
                dto.ArrImage = recipe.ImageUrl;

            dto.Tags = DeserializeTags(recipe.Tags);

            var comments = allActions
                .Where(ua => ua.RecipeId == recipe.Id && ua.ActionType == UserActionType.Comment)
                .ToList();

            dto.CommentCount = comments.Count;
            dto.AverageRating = comments.Count > 0
                ? comments
                    .Where(ua => ua.Rating.HasValue)
                    .Select(ua => (double)ua.Rating!.Value)
                    .DefaultIfEmpty(0)
                    .Average()
                : null;

            return dto;
        }

        private static string? SerializeTags(List<string>? tags)
        {
            if (tags == null || tags.Count == 0) return null;
            return JsonSerializer.Serialize(tags.Select(t => t.Trim().ToLowerInvariant()).Distinct().ToList());
        }

        private static List<string> DeserializeTags(string? tagsJson)
        {
            if (string.IsNullOrEmpty(tagsJson)) return new List<string>();
            try { return JsonSerializer.Deserialize<List<string>>(tagsJson) ?? new List<string>(); }
            catch { return new List<string>(); }
        }
    }
}