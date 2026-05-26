using AutoMapper;
using RecipeApp.Common.DTOs;
using RecipeApp.Repository.Entities;
using RecipeApp.Repository.Interfaces;
using RecipeApp.Services.Interfaces;

namespace RecipeApp.Services.Services
{
    public class IngredientService : IIngredientService
    {
        private readonly IRepository<Ingredient> _repo;
        private readonly IMapper _mapper;

        public IngredientService(IRepository<Ingredient> ingredientRepository, IMapper mapper)
        {
            _repo = ingredientRepository;
            _mapper = mapper;
        }

        public async Task<List<IngredientDto>> GetAll()
            => _mapper.Map<List<IngredientDto>>(await _repo.GetAll());

        public async Task<IngredientDto> GetById(int id)
        {
            var ingredient = await _repo.GetById(id)
                ?? throw new KeyNotFoundException($"Ingredient with id {id} not found.");
            return _mapper.Map<IngredientDto>(ingredient);
        }
        
        
        public async Task DeleteItem(int id)
        {
            if (await _repo.GetById(id) == null)
                throw new KeyNotFoundException($"Ingredient with id {id} not found.");
            await _repo.DeleteItem(id);
        }

        public async Task<IngredientDto> GetByName(string name)
        {
            var all = await _repo.GetAll();
            var match = all.FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
            return match != null ? _mapper.Map<IngredientDto>(match) : null!;
        }

        public async Task<IngredientDto> CreateIngredient(IngredientCreateDto dto)
        {
            if (await GetByName(dto.Name) != null)
                throw new InvalidOperationException($"Ingredient '{dto.Name}' already exists.");

            return _mapper.Map<IngredientDto>(await _repo.AddItem(_mapper.Map<Ingredient>(dto)));
        }

        public async Task<IngredientDto> UpdateIngredient(int id, IngredientUpdateDto dto)
        {
            var existing = await _repo.GetById(id)
                ?? throw new KeyNotFoundException($"Ingredient with id {id} not found.");

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                if (!string.Equals(existing.Name, dto.Name, StringComparison.OrdinalIgnoreCase))
                {
                    if (await GetByName(dto.Name) != null)
                        throw new InvalidOperationException($"Ingredient '{dto.Name}' already exists.");
                }
                existing.Name = dto.Name;
            }

            return _mapper.Map<IngredientDto>(await _repo.UpdateItem(id, existing));
        }
    }
}