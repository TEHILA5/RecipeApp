using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipeApp.Common.DTOs;

namespace RecipeApp.Services.Interfaces
{
    public interface IConversionService : IService<ConversionDto>
    {
        Task<ConversionDto> FindConversion(int ingredientId1, int ingredientId2);
        Task<ConversionDto> CreateConversion(ConversionCreateDto createDto);
    }
}
