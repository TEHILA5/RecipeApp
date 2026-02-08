using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using RecipeApp.Common.DTOs;
using RecipeApp.Repository.Entities;
using RecipeApp.Repository.Interfaces;
using RecipeApp.Repository.Repositories;
using RecipeApp.Services.Interfaces;

namespace RecipeApp.Services.Services
{
    public class ConversionService : IConversionService
    {
        private readonly IRepository<Conversion> _conversionRepository;
        private readonly IRepository<Ingredient> _ingredientRepository;
        private readonly IMapper _mapper;

        public ConversionService(
            IRepository<Conversion> conversionRepository,
            IRepository<Ingredient> ingredientRepository,
            IMapper mapper)
        {
            _conversionRepository = conversionRepository;
            _ingredientRepository = ingredientRepository;
            _mapper = mapper;
        }

        //   Generic CRUD  
        public async Task<List<ConversionDto>> GetAll()
        {
            var conversions = await _conversionRepository.GetAll();
            return await EnrichConversions(conversions);
        }

        public async Task<ConversionDto> GetById(int id)
        {
            var conversion = await _conversionRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Conversion with id {id} not found.");

            var enriched = await EnrichConversions(new[] { conversion });
            return enriched.First();
        }

        public async Task<ConversionDto> AddItem(ConversionDto item)
        {
            var ingredients = await _ingredientRepository.GetAll();
            var id1 = GetIngredientIdByName(ingredients, item.Ingredient1Name);
            var id2 = GetIngredientIdByName(ingredients, item.Ingredient2Name);

            var existing = await FindConversion(id1, id2);
            if (existing != null)
                throw new InvalidOperationException(
                    $"Conversion between '{item.Ingredient1Name}' and '{item.Ingredient2Name}' already exists.");

            var conversion = _mapper.Map<Conversion>(item);
            var created = await _conversionRepository.AddItem(conversion);

            var enriched = await EnrichConversions(new[] { created });
            return enriched.First();
        }

        public async Task<ConversionDto> UpdateItem(int id, ConversionDto item)
        {
            var existing = await _conversionRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Conversion with id {id} not found.");

            var ingredients = await _ingredientRepository.GetAll();
             
            if (item.ConversionRatio.HasValue)
                existing.ConversionRatio = item.ConversionRatio.Value;

            if (item.IsBidirectional.HasValue)
                existing.IsBidirectional = item.IsBidirectional.Value;

            var updated = await _conversionRepository.UpdateItem(id, existing);

            var enriched = await EnrichConversions(new[] { updated });
            return enriched.First();
        }


        public async Task DeleteItem(int id)
        { 
            var existing = await _conversionRepository.GetById(id);
            if (existing == null)
                throw new KeyNotFoundException($"Conversion with id {id} not found.");
            await _conversionRepository.DeleteItem(id);
        }

        //  Conversion-Specific  
        /// <summary>
        /// מחפש רכיב להמרה.  
        /// בודקת קודם כיוון ישיר, ואם לא נמצא – כיוון הפוך. 
        /// </summary>

        public async Task<ConversionDto> FindConversion(int ingredientId1, int ingredientId2)
        {
            var conversions = await _conversionRepository.GetAll();
            var ingredients = await _ingredientRepository.GetAll();
            var ingredientDict = ingredients.ToDictionary(i => i.Id);

            // כיוון ישיר: 
            //1->2
            var direct = conversions.FirstOrDefault(c =>
                c.IngredientId1 == ingredientId1 && c.IngredientId2 == ingredientId2);

            if (direct != null)
                return BuildDto(direct, ingredientDict);

            // כיוון הפוך: חפש 
            //2->1
            var reverse = conversions.FirstOrDefault(c =>
                c.IngredientId1 == ingredientId2 && c.IngredientId2 == ingredientId1);

            if (reverse != null && reverse.IsBidirectional)
            {
                return new ConversionDto
                {
                    Id = reverse.Id,
                    Ingredient1Name = ingredientDict.ContainsKey(ingredientId1)
                        ? ingredientDict[ingredientId1].Name : "Unknown",
                    Ingredient2Name = ingredientDict.ContainsKey(ingredientId2)
                        ? ingredientDict[ingredientId2].Name : "Unknown",
                    ConversionRatio = 1m / reverse.ConversionRatio,
                    IsBidirectional = true
                };
            }

            return null!;
        }

        /// <summary>
        /// יצירת המרה חדשה (Admin בלבד)
        /// </summary>
        public async Task<ConversionDto> CreateConversion(ConversionCreateDto createDto)
        {
            // בדיקה שהרכיבים קיימים
            var ingredient1 = await _ingredientRepository.GetById(createDto.IngredientId1);
            if (ingredient1 == null)
                throw new KeyNotFoundException($"Ingredient with id {createDto.IngredientId1} not found.");

            var ingredient2 = await _ingredientRepository.GetById(createDto.IngredientId2);
            if (ingredient2 == null)
                throw new KeyNotFoundException($"Ingredient with id {createDto.IngredientId2} not found.");

            // בדיקה שההמרה לא קיימת
            var existing = await FindConversion(createDto.IngredientId1, createDto.IngredientId2);
            if (existing != null)
                throw new InvalidOperationException(
                    $"Conversion between '{ingredient1.Name}' and '{ingredient2.Name}' already exists.");

            var conversion = _mapper.Map<Conversion>(createDto);
            var created = await _conversionRepository.AddItem(conversion);

            var enriched = await EnrichConversions(new[] { created });
            return enriched.First();
        }

        /// <summary>
        /// עדכון המרה (Admin בלבד)
        /// </summary>
        public async Task<ConversionDto> UpdateConversion(int id, ConversionUpdateDto updateDto)
        {
            var existing = await _conversionRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Conversion with id {id} not found.");

            // עדכן רק את השדות שסופקו
            if (updateDto.ConversionRatio.HasValue)
            {
                existing.ConversionRatio = updateDto.ConversionRatio.Value;
            }

            if (updateDto.IsBidirectional.HasValue)
            {
                existing.IsBidirectional = updateDto.IsBidirectional.Value;
            }

            var updated = await _conversionRepository.UpdateItem(id, existing);

            var enriched = await EnrichConversions(new[] { updated });
            return enriched.First();
        }

        //  Helpers  
        private async Task<List<ConversionDto>> EnrichConversions(IEnumerable<Conversion> conversions)
        {
            var ingredients = await _ingredientRepository.GetAll();
            var ingredientDict = ingredients.ToDictionary(i => i.Id);
            return conversions.Select(c => BuildDto(c, ingredientDict)).ToList();
        }

        private static ConversionDto BuildDto(Conversion conversion, Dictionary<int, Ingredient> ingredientDict)
        {
            return new ConversionDto
            {
                Id = conversion.Id,
                Ingredient1Name = ingredientDict.ContainsKey(conversion.IngredientId1)
                    ? ingredientDict[conversion.IngredientId1].Name : "Unknown",
                Ingredient2Name = ingredientDict.ContainsKey(conversion.IngredientId2)
                    ? ingredientDict[conversion.IngredientId2].Name : "Unknown",
                ConversionRatio = conversion.ConversionRatio,
                IsBidirectional = conversion.IsBidirectional
            };
        }

        private static int GetIngredientIdByName(List<Ingredient> ingredients, string name)
        {
            var ingredient = ingredients
                .FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase))
                ?? throw new KeyNotFoundException($"Ingredient '{name}' not found.");
            return ingredient.Id;
        }
    }
}
