using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipeApp.Common.DTOs;

namespace RecipeApp.Services.Interfaces
{
    public interface IUserActionService : IService<UserActionDto>
    { 
        Task<List<UserActionDto>> GetUserComments(int userId);
        Task<List<UserActionDto>> GetUserHistory(int userId);
        Task<List<UserActionDto>> GetUserSavedRecipes(int userId);
        Task<UserPreferencesDto> GetUserPreferences(int userId);
        // יצירה לפי סוג
        Task<UserActionDto> CreateComment(int userId, CommentCreateDto createDto);
        Task<UserActionDto> CreateBook(int userId, BookCreateDto createDto);
        Task<UserActionDto> CreateHistory(int userId, HistoryCreateDto createDto);
    }
}
