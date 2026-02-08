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
    public class IngredientService : IIngredientService
    {
        private readonly IRepository<Ingredient> _ingredientRepository;
        private readonly IMapper _mapper;

        public IngredientService(IRepository<Ingredient> ingredientRepository, IMapper mapper)
        {
            _ingredientRepository = ingredientRepository;
            _mapper = mapper;
        }

        //  Generic CRUD  
        public async Task<List<IngredientDto>> GetAll()
        {
            var ingredients = await _ingredientRepository.GetAll();
            return _mapper.Map<List<IngredientDto>>(ingredients);
        }

        public async Task<IngredientDto> GetById(int id)
        {
            var ingredient = await _ingredientRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Ingredient with id {id} not found.");
            return _mapper.Map<IngredientDto>(ingredient);
        }

        public async Task<IngredientDto> AddItem(IngredientDto item)
        {
            var existing = await GetByName(item.Name);
            if (existing != null)
                throw new InvalidOperationException($"Ingredient '{item.Name}' already exists.");

            var ingredient = _mapper.Map<Ingredient>(item);
            var created = await _ingredientRepository.AddItem(ingredient);
            return _mapper.Map<IngredientDto>(created);
        }

        public async Task<IngredientDto> UpdateItem(int id, IngredientDto item)
        {
            var existing = await _ingredientRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Ingredient with id {id} not found.");

            if (!string.Equals(existing.Name, item.Name, StringComparison.OrdinalIgnoreCase))
            {
                var duplicate = await GetByName(item.Name);
                if (duplicate != null)
                    throw new InvalidOperationException($"Ingredient '{item.Name}' already exists.");
            }

            _mapper.Map(item, existing);
            var updated = await _ingredientRepository.UpdateItem(id, existing);
            return _mapper.Map<IngredientDto>(updated);
        }

        public async Task DeleteItem(int id)
        {  
            var existing = await _ingredientRepository.GetById(id);
            if (existing == null)
                throw new KeyNotFoundException($"Ingredient with id {id} not found.");
            await _ingredientRepository.DeleteItem(id);
        }

        //   Ingredient-Specific  
        public async Task<IngredientDto> GetByName(string name)
        {
            var ingredients = await _ingredientRepository.GetAll();
            var ingredient = ingredients
                .FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));

            return ingredient != null ? _mapper.Map<IngredientDto>(ingredient) : null!;
        }

        /// <summary>
        /// יצירת רכיב חדש (Admin בלבד)
        /// </summary>
        public async Task<IngredientDto> CreateIngredient(IngredientCreateDto createDto)
        {
            var existing = await GetByName(createDto.Name);
            if (existing != null)
                throw new InvalidOperationException($"Ingredient '{createDto.Name}' already exists.");

            var ingredient = _mapper.Map<Ingredient>(createDto);
            var created = await _ingredientRepository.AddItem(ingredient);
            return _mapper.Map<IngredientDto>(created);
        }
        /// <summary>
        ///   עדכון רכיב 
        /// </summary>
        public async Task<IngredientDto> UpdateIngredient(int id, IngredientUpdateDto updateDto)
        {
            var existing = await _ingredientRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Ingredient with id {id} not found.");
             
            if (!string.IsNullOrWhiteSpace(updateDto.Name))
            { 
                if (!string.Equals(existing.Name, updateDto.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var duplicate = await GetByName(updateDto.Name);
                    if (duplicate != null)
                        throw new InvalidOperationException($"Ingredient '{updateDto.Name}' already exists.");
                }

                existing.Name = updateDto.Name;
            }

            var updated = await _ingredientRepository.UpdateItem(id, existing);
            return _mapper.Map<IngredientDto>(updated);
        }
    }
}
