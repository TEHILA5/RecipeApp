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
    }
}
