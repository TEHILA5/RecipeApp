using Microsoft.EntityFrameworkCore;
using RecipeApp.Repository.Entities;
using RecipeApp.Repository.Interfaces;

namespace RecipeApp.Repository.Repositories
{
    public class RecipeRepository : IRepository<Recipe>
    {
        private readonly IContext ctx;

        public RecipeRepository(IContext context)
        {
            ctx = context;
        }

        public async Task<List<Recipe>> GetAll()
        {
            return await ctx.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .ToListAsync();
        }

        public async Task<Recipe> GetById(int id)
        {
            return await ctx.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Recipe> AddItem(Recipe item)
        {
            ctx.Recipes.Add(item);
            await ctx.Save();
            return await ctx.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(x => x.Id == item.Id);
        }

        public async Task<Recipe> UpdateItem(int id, Recipe recipe)
        {
            // ExecuteDeleteAsync/ExecuteUpdateAsync עוקפים את ה-tracking לחלוטין
            await ctx.RecipeIngredients
                .Where(ri => ri.RecipeId == id)
                .ExecuteDeleteAsync();

            await ctx.Recipes
                .Where(r => r.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.Name, recipe.Name)
                    .SetProperty(r => r.Description, recipe.Description)
                    .SetProperty(r => r.Category, recipe.Category)
                    .SetProperty(r => r.Instructions, recipe.Instructions)
                    .SetProperty(r => r.ImageUrl, recipe.ImageUrl)
                    .SetProperty(r => r.Servings, recipe.Servings)
                    .SetProperty(r => r.Level, recipe.Level)
                    .SetProperty(r => r.PrepTime, recipe.PrepTime)
                    .SetProperty(r => r.TotalTime, recipe.TotalTime)
                    .SetProperty(r => r.Tags, recipe.Tags)
                );

            if (recipe.RecipeIngredients?.Any() == true)
            {
                var newIngredients = recipe.RecipeIngredients.Select(ri => new RecipeIngredient
                {
                    RecipeId = id,
                    IngredientId = ri.IngredientId,
                    Quantity = ri.Quantity,
                    Unit = ri.Unit,
                    Importance = ri.Importance
                }).ToList();

                ctx.RecipeIngredients.AddRange(newIngredients);
                await ctx.Save();
            }

            return await ctx.Recipes
                .Include(x => x.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task DeleteItem(int id)
        {
            var recipe = await ctx.Recipes.FirstOrDefaultAsync(x => x.Id == id);
            ctx.Recipes.Remove(recipe);
            await ctx.Save();
        }
    }
}