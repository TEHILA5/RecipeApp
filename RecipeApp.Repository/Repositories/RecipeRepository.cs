using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        // ⭐ תיקון: לטעון בחזרה את המתכון עם ה-Ingredients
        public async Task<Recipe> AddItem(Recipe item)
        {
            ctx.Recipes.Add(item);
            await ctx.Save();

            // טוען בחזרה את המתכון עם כל ה-Ingredients
            return await ctx.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(x => x.Id == item.Id);
        }

        public async Task<Recipe> UpdateItem(int id, Recipe recipe)
        {
            var r = await ctx.Recipes
               .Include(x => x.RecipeIngredients)
                   .ThenInclude(ri => ri.Ingredient)
               .FirstOrDefaultAsync(x => x.Id == id);

            r.Name = recipe.Name;
            r.Description = recipe.Description;
            r.Category = recipe.Category;
            r.Instructions = recipe.Instructions;
            r.ImageUrl = recipe.ImageUrl;
            r.Servings = recipe.Servings;
            r.Level = recipe.Level;
            r.PrepTime = recipe.PrepTime;
            r.TotalTime = recipe.TotalTime;

            await ctx.Save();
             
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