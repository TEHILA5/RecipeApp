using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipeApp.Common.DTOs;

namespace RecipeApp.Services.Interfaces
{
    public interface IRecipeService : IService<RecipeDto>
    { 
        Task<List<RecipeDto>> SearchByCategory(string category);
        Task<List<RecipeDto>> SearchByIngredients(List<string> ingredients);
        Task<List<RecipeDto>> GetRecommendedForUser(int userId);
    }
}
