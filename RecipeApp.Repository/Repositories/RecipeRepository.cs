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
            return await ctx.Recipes.ToListAsync();
        }

        public async Task<Recipe> GetById(int id)
        {
            return await ctx.Recipes.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Recipe> AddItem(Recipe item)
        {
            ctx.Recipes.Add(item);
            await ctx.Save();
            return item;
        }

        public async Task<Recipe> UpdateItem(int id, Recipe recipe)
        {
            var r = await ctx.Recipes.FirstOrDefaultAsync(x => x.Id == id);
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
            return r;
        }

        public async Task DeleteItem(int id)
        {
            var recipe = await ctx.Recipes.FirstOrDefaultAsync(x => x.Id == id);
            ctx.Recipes.Remove(recipe);
            await ctx.Save();
        }
    }
}
