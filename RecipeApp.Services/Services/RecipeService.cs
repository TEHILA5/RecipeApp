using AutoMapper;
using RecipeApp.Common.DTOs;
using RecipeApp.Repository.Entities;
using RecipeApp.Repository.Interfaces;
using RecipeApp.Repository.Repositories;
using RecipeApp.Services.Interfaces;

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

        // Generic CRUD  
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

            _mapper.Map(item, existing);
            var updated = await _recipeRepository.UpdateItem(id, existing);

            var allActions = await _userActionRepository.GetAll();
            return MapRecipeWithStats(updated, allActions);
        }

        public async Task DeleteItem(int id)
        {
            var existing = await _recipeRepository.GetById(id);
            if (existing == null)
                throw new KeyNotFoundException($"Recipe with id {id} not found.");
            await _recipeRepository.DeleteItem(id);
        }

        // Recipe-Specific  
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

        /// <summary>
        /// מחזיר מתכונים שמכילים את כל המצרכים האלו.
        /// </summary>
        public async Task<List<RecipeDto>> SearchByIngredients(List<string> ingredients)
        {
            var allIngredients = await _ingredientRepository.GetAll();

            var ingredientIds = allIngredients
                .Where(i => ingredients.Any(name =>
                    string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase)))
                .Select(i => i.Id)
                .ToHashSet();

            if (ingredientIds.Count == 0)
                return new List<RecipeDto>();

            var recipes = await _recipeRepository.GetAll();
            var allActions = await _userActionRepository.GetAll();

            return recipes
                .Where(r => r.RecipeIngredients != null
                    && ingredientIds.All(id =>
                        r.RecipeIngredients.Any(ri => ri.IngredientId == id)))
                .Select(r => MapRecipeWithStats(r, allActions))
                .ToList();
        }

        /// <summary>
        /// מוצא את הקטגוריות הכי נפוצות של המשתמש
        /// </summary>
        public async Task<List<RecipeDto>> GetRecommendedForUser(int userId)
        {
            var allActions = await _userActionRepository.GetAll();
            var myActions = allActions.Where(ua => ua.UserId == userId).ToList();
            var recipes = await _recipeRepository.GetAll();

            if (myActions.Count == 0)
                return recipes.Select(r => MapRecipeWithStats(r, allActions)).ToList();

            var recipeDictionary = recipes.ToDictionary(r => r.Id);

            var seenRecipeIds = myActions
                .Where(ua => ua.RecipeId.HasValue)
                .Select(ua => ua.RecipeId!.Value)
                .ToHashSet();

            // מוצא קטגוריה הכי נפוצה
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

            List<Recipe> recommended;

            if (mostCommonCategory.HasValue)
            {
                recommended = recipes
                    .Where(r => r.Category == mostCommonCategory.Value
                        && !seenRecipeIds.Contains(r.Id))
                    .ToList();
            }
            else
            {
                recommended = recipes
                    .Where(r => !seenRecipeIds.Contains(r.Id))
                    .ToList();
            }

            return recommended.Select(r => MapRecipeWithStats(r, allActions)).ToList();
        }

        //  Helpers  
        /// <summary>
        /// מוסיף למתכון מונה תגובות וממוצע דירוגים
        /// </summary>
        private RecipeDto MapRecipeWithStats(Recipe recipe, List<UserAction> allActions)
        {
            var dto = _mapper.Map<RecipeDto>(recipe);

            var recipeComments = allActions
                .Where(ua => ua.RecipeId == recipe.Id && ua.ActionType == UserActionType.Comment)
                .ToList();

            dto.CommentCount = recipeComments.Count;
            dto.AverageRating = recipeComments.Count > 0
                ? recipeComments
                    .Where(ua => ua.Rating.HasValue)
                    .Select(ua => (double)ua.Rating!.Value)
                    .DefaultIfEmpty(0)
                    .Average()
                : null;

            return dto;
        }
    }
}
