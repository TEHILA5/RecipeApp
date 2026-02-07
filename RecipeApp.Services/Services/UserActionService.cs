using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using RecipeApp.Common.DTOs;
using RecipeApp.Repository.Entities;
using RecipeApp.Repository.Interfaces;
using RecipeApp.Services.Interfaces;

namespace RecipeApp.Services.Services
{
    public class UserActionService : IUserActionService
    {
        private readonly IRepository<UserAction> _userActionRepository;
        private readonly IRepository<Recipe> _recipeRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IMapper _mapper;

        public UserActionService(
            IRepository<UserAction> userActionRepository,
            IRepository<Recipe> recipeRepository,
            IRepository<User> userRepository,
            IMapper mapper)
        {
            _userActionRepository = userActionRepository;
            _recipeRepository = recipeRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        //   Generic CRUD  
        public async Task<List<UserActionDto>> GetAll()
        {
            var actions = await _userActionRepository.GetAll();
            return await EnrichActions(actions);
        }

        public async Task<UserActionDto> GetById(int id)
        {
            var action = await _userActionRepository.GetById(id)
                ?? throw new KeyNotFoundException($"UserAction with id {id} not found.");

            var enriched = await EnrichActions(new[] { action });
            return enriched.First();
        }

        public async Task<UserActionDto> AddItem(UserActionDto item)
        {
            var action = _mapper.Map<UserAction>(item);
            var created = await _userActionRepository.AddItem(action);

            var enriched = await EnrichActions(new[] { created });
            return enriched.First();
        }

        public async Task<UserActionDto> UpdateItem(int id, UserActionDto item)
        {
            var existing = await _userActionRepository.GetById(id)
                ?? throw new KeyNotFoundException($"UserAction with id {id} not found.");

            _mapper.Map(item, existing);
            var updated = await _userActionRepository.UpdateItem(id, existing);

            var enriched = await EnrichActions(new[] { updated });
            return enriched.First();
        }

        public async Task DeleteItem(int id)
        { 
            var existing = await _userActionRepository.GetById(id);
            if (existing == null)
                throw new KeyNotFoundException($"UserAction with id {id} not found.");
            await _userActionRepository.DeleteItem(id);
        }

        // Create Methods
         
        // יצירת תגובה חדשה
        public async Task<UserActionDto> CreateComment(int userId, CommentCreateDto createDto)
        { 
            var recipe = await _recipeRepository.GetById(createDto.RecipeId);
            if (recipe == null)
                throw new KeyNotFoundException($"Recipe with id {createDto.RecipeId} not found.");
             
            var userAction = _mapper.Map<UserAction>(createDto);
            userAction.UserId = userId;
            userAction.CreatedAt = DateTime.UtcNow;
             
            var created = await _userActionRepository.AddItem(userAction);
             
            var enriched = await EnrichActions(new[] { created });
            return enriched.First();
        }
         
        // שמירת מתכון
        public async Task<UserActionDto> CreateBook(int userId, BookCreateDto createDto)
        { 
            var recipe = await _recipeRepository.GetById(createDto.RecipeId);
            if (recipe == null)
                throw new KeyNotFoundException($"Recipe with id {createDto.RecipeId} not found.");
             
            var allActions = await _userActionRepository.GetAll();
            var alreadySaved = allActions.Any(ua =>
                ua.UserId == userId &&
                ua.ActionType == UserActionType.Book &&
                ua.RecipeId == createDto.RecipeId);

            if (alreadySaved)
                throw new InvalidOperationException("Recipe is already saved.");
             
            var userAction = _mapper.Map<UserAction>(createDto);
            userAction.UserId = userId;
            userAction.CreatedAt = DateTime.UtcNow;
             
            var created = await _userActionRepository.AddItem(userAction);
             
            var enriched = await EnrichActions(new[] { created });
            return enriched.First();
        }
         
        // רישום חיפוש בהיסטוריה
        public async Task<UserActionDto> CreateHistory(int userId, HistoryCreateDto createDto)
        { 
            var userAction = _mapper.Map<UserAction>(createDto);
            userAction.UserId = userId;
            userAction.CreatedAt = DateTime.UtcNow;
             
            var created = await _userActionRepository.AddItem(userAction);
             
            var enriched = await EnrichActions(new[] { created });
            return enriched.First();
        }

        //  UserAction-Specific  
        public async Task<List<UserActionDto>> GetUserComments(int userId)
        {
            var actions = await _userActionRepository.GetAll();
            var comments = actions
                .Where(ua => ua.UserId == userId && ua.ActionType == UserActionType.Comment)
                .ToList();

            return await EnrichActions(comments);
        }

        /// <summary>
        /// היסטוריית משתמש
        /// </summary>
        public async Task<List<UserActionDto>> GetUserHistory(int userId)
        {
            var actions = await _userActionRepository.GetAll();
            var history = actions
                .Where(ua => ua.UserId == userId && ua.ActionType == UserActionType.History)
                .OrderByDescending(ua => ua.CreatedAt)
                .ToList();

            return await EnrichActions(history);
        }

        public async Task<List<UserActionDto>> GetUserSavedRecipes(int userId)
        {
            var actions = await _userActionRepository.GetAll();
            var saved = actions
                .Where(ua => ua.UserId == userId && ua.ActionType == UserActionType.Book)
                .ToList();

            return await EnrichActions(saved);
        }

        /// <summary>
        /// יוצר העדפות משתמש לפי ההיסטוריה
        /// - FavoriteCategory: הקטגוריה הכי נפוצה אצלו
        /// - CategoryStats: מונה לכל קטגוריה
        /// </summary>
        public async Task<UserPreferencesDto> GetUserPreferences(int userId)
        {
            var actions = await _userActionRepository.GetAll();

            var historyActions = actions
                .Where(ua => ua.UserId == userId && ua.ActionType == UserActionType.History)
                .ToList();

            var categoryStats = historyActions
                .GroupBy(ua => ua.Category)
                .Where(g => g.Key.HasValue)
                .Select(g => new CategoryStatsDto
                {
                    Category = g.Key!.Value,
                    SearchCount = g.Count()
                })
                .OrderByDescending(s => s.SearchCount)
                .ToList();

            var favoriteCategory = categoryStats.FirstOrDefault()?.Category ?? default;

            return new UserPreferencesDto
            {
                FavoriteCategory = favoriteCategory,
                CategoryStats = categoryStats
            };
        }

        //Helpers 
        /// <summary>
        /// כל פעולה ברשימה מקבלת גם:
        /// RecipeName, Category, UserName.
        /// </summary>
        private async Task<List<UserActionDto>> EnrichActions(IEnumerable<UserAction> actions)
        {
            var actionList = actions.ToList();
            if (actionList.Count == 0)
                return new List<UserActionDto>();

            var recipes = await _recipeRepository.GetAll();
            var users = await _userRepository.GetAll();

            var recipeDictionary = recipes.ToDictionary(r => r.Id);
            var userDictionary = users.ToDictionary(u => u.Id);

            return actionList.Select(action =>
            {
                var dto = _mapper.Map<UserActionDto>(action);

                if (userDictionary.TryGetValue(action.UserId, out var user))
                    dto.UserName = user.Name;

                if (action.RecipeId.HasValue
                    && recipeDictionary.TryGetValue(action.RecipeId.Value, out var recipe))
                {
                    dto.RecipeName = recipe.Name;
                    dto.Category = recipe.Category;
                    if (!string.IsNullOrEmpty(recipe.ImageUrl))
                    {
                        dto.RecipeImageUrl = $"data:image/jpeg;base64,{recipe.ImageUrl}";
                    }
                }

                return dto;
            }).ToList();
        }
    }
}
