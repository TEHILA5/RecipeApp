using AutoMapper;
using RecipeApp.Common.DTOs;
using RecipeApp.Repository.Entities;
using RecipeApp.Repository.Interfaces;
using RecipeApp.Services.Interfaces;

namespace RecipeApp.Services.Services
{
    public class ConversionService : IConversionService
    {
        private readonly IRepository<Conversion> _repo;
        private readonly IRepository<Ingredient> _ingredients;
        private readonly IMapper _mapper;

        public ConversionService(
            IRepository<Conversion> conversionRepository,
            IRepository<Ingredient> ingredientRepository,
            IMapper mapper)
        {
            _repo = conversionRepository;
            _ingredients = ingredientRepository;
            _mapper = mapper;
        }

        public async Task<List<ConversionDto>> GetAll()
        {
            return await Enrich(await _repo.GetAll());
        }

        public async Task<ConversionDto> GetById(int id)
        {
            var conversion = await _repo.GetById(id)
                ?? throw new KeyNotFoundException($"Conversion with id {id} not found.");
            return (await Enrich(new[] { conversion })).First();
        }

        public async Task DeleteItem(int id)
        {
            if (await _repo.GetById(id) == null)
                throw new KeyNotFoundException($"Conversion with id {id} not found.");
            await _repo.DeleteItem(id);
        }

        public async Task<ConversionDto> FindConversion(int ingredientId1, int ingredientId2)
        {
            var conversions = await _repo.GetAll();
            var dict = (await _ingredients.GetAll()).ToDictionary(i => i.Id);

            var direct = conversions.FirstOrDefault(c =>
                c.IngredientId1 == ingredientId1 && c.IngredientId2 == ingredientId2);

            if (direct != null)
                return BuildDto(direct, dict);

            var reverse = conversions.FirstOrDefault(c =>
                c.IngredientId1 == ingredientId2 && c.IngredientId2 == ingredientId1);

            if (reverse != null && reverse.IsBidirectional)
            {
                return new ConversionDto
                {
                    Id = reverse.Id,
                    Ingredient1Name = dict.ContainsKey(ingredientId1) ? dict[ingredientId1].Name : "Unknown",
                    Ingredient2Name = dict.ContainsKey(ingredientId2) ? dict[ingredientId2].Name : "Unknown",
                    ConversionRatio = 1m / reverse.ConversionRatio,
                    IsBidirectional = true
                };
            }

            return null!;
        }

        public async Task<ConversionDto> CreateConversion(ConversionCreateDto dto)
        {
            var ing1 = await _ingredients.GetById(dto.IngredientId1)
                ?? throw new KeyNotFoundException($"Ingredient with id {dto.IngredientId1} not found.");

            var ing2 = await _ingredients.GetById(dto.IngredientId2)
                ?? throw new KeyNotFoundException($"Ingredient with id {dto.IngredientId2} not found.");

            if (await FindConversion(dto.IngredientId1, dto.IngredientId2) != null)
                throw new InvalidOperationException(
                    $"Conversion between '{ing1.Name}' and '{ing2.Name}' already exists.");

            var created = await _repo.AddItem(_mapper.Map<Conversion>(dto));
            return (await Enrich(new[] { created })).First();
        }

        public async Task<ConversionDto> UpdateConversion(int id, ConversionUpdateDto dto)
        {
            var existing = await _repo.GetById(id)
                ?? throw new KeyNotFoundException($"Conversion with id {id} not found.");

            if (dto.ConversionRatio.HasValue)
                existing.ConversionRatio = dto.ConversionRatio.Value;

            if (dto.IsBidirectional.HasValue)
                existing.IsBidirectional = dto.IsBidirectional.Value;

            return (await Enrich(new[] { await _repo.UpdateItem(id, existing) })).First();
        }

        private async Task<List<ConversionDto>> Enrich(IEnumerable<Conversion> conversions)
        {
            var dict = (await _ingredients.GetAll()).ToDictionary(i => i.Id);
            return conversions.Select(c => BuildDto(c, dict)).ToList();
        }

        private static ConversionDto BuildDto(Conversion c, Dictionary<int, Ingredient> dict) => new()
        {
            Id = c.Id,
            Ingredient1Name = dict.ContainsKey(c.IngredientId1) ? dict[c.IngredientId1].Name : "Unknown",
            Ingredient2Name = dict.ContainsKey(c.IngredientId2) ? dict[c.IngredientId2].Name : "Unknown",
            ConversionRatio = c.ConversionRatio,
            IsBidirectional = c.IsBidirectional
        };

        private static int GetIdByName(List<Ingredient> ingredients, string name)
        {
            return (ingredients.FirstOrDefault(i =>
                string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase))
                    ?? throw new KeyNotFoundException($"Ingredient '{name}' not found.")).Id;
        }
    }
}