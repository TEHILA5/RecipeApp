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

            if (item.Name != null)
                existing.Name = item.Name;
            if (item.Description != null)
                existing.Description = item.Description;
            if (item.Category.HasValue)
                existing.Category = item.Category.Value;
            if (item.Instructions != null)
                existing.Instructions = item.Instructions;
            if (item.ArrImage?.Length > 0)
                existing.ImageUrl = item.ArrImage;
            if (item.Servings.HasValue)
                existing.Servings = item.Servings.Value;
            if (item.Level.HasValue)
                existing.Level = item.Level.Value;
            if (item.PrepTime.HasValue)
                existing.PrepTime = item.PrepTime.Value;
            if (item.TotalTime.HasValue)
                existing.TotalTime = item.TotalTime.Value;

            if (item.Ingredients != null)
            {
                foreach (var ri in item.Ingredients)
                {
                    var existingIngredient = existing.RecipeIngredients
                        .FirstOrDefault(x => x.IngredientId == ri.IngredientId);

                    if (existingIngredient != null)
                    { 
                        existingIngredient.Quantity = ri.Quantity;
                        existingIngredient.Unit = ri.Unit;
                    }
                    else
                    { 
                        existing.RecipeIngredients.Add(new RecipeIngredient
                        {
                            IngredientId = ri.IngredientId,
                            Quantity = ri.Quantity,
                            Unit = ri.Unit
                        });
                    }
                }
            }
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

        // Recipe-Specific Create/Update 

        /// <summary>
        /// יצירת מתכון חדש (Admin בלבד)
        /// </summary>
        public async Task<RecipeDto> CreateRecipe(RecipeCreateDto createDto)
        {
            var recipe = new Recipe
            {
                Name = createDto.Name,
                Description = createDto.Description,
                Category = createDto.Category,
                Instructions = createDto.Instructions,
                ImageUrl = createDto.ArrImage,
                Servings = createDto.Servings,
                Level = createDto.Level,
                PrepTime = createDto.PrepTime,
                TotalTime = createDto.TotalTime,
                RecipeIngredients = createDto.Ingredients?.Select(i => new RecipeIngredient
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

        /// <summary>
        /// עדכון מתכון (Admin בלבד)
        /// </summary>
        public async Task<RecipeDto> UpdateRecipe(int id, RecipeCreateDto updateDto)
        {
            var existing = await _recipeRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Recipe with id {id} not found.");

            existing.Name = updateDto.Name;
            existing.Description = updateDto.Description;
            existing.Category = updateDto.Category;
            existing.Instructions = updateDto.Instructions;
            existing.ImageUrl = updateDto.ArrImage;
            existing.Servings = updateDto.Servings;
            existing.Level = updateDto.Level;
            existing.PrepTime = updateDto.PrepTime;
            existing.TotalTime = updateDto.TotalTime;
             
            if (updateDto.Ingredients != null)
            { 
                existing.RecipeIngredients.Clear();
                 
                existing.RecipeIngredients = updateDto.Ingredients.Select(i => new RecipeIngredient
                {
                    IngredientId = i.IngredientId,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Importance = i.Importance
                }).ToList();
            }

            var updated = await _recipeRepository.UpdateItem(id, existing);

            var allActions = await _userActionRepository.GetAll();
            return MapRecipeWithStats(updated, allActions);
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

            if (!string.IsNullOrEmpty(recipe.ImageUrl))
            {
                dto.ArrImage =  recipe.ImageUrl;
            }

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
