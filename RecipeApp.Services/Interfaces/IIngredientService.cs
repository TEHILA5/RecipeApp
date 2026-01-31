using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipeApp.Common.DTOs;

namespace RecipeApp.Services.Interfaces
{
    public interface IIngredientService : IService<IngredientDto>
    {
        Task<IngredientDto> GetByName(string name);
    }
}
