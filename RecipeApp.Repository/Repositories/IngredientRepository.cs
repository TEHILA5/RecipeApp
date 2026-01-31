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
    public class IngredientRepository : IRepository<Ingredient>
    {
        private readonly IContext ctx;

        public IngredientRepository(IContext context)
        {
            ctx = context;
        }

        public async Task<List<Ingredient>> GetAll()
        {
            return await ctx.Ingredients.ToListAsync();
        }

        public async Task<Ingredient> GetById(int id)
        {
            return await ctx.Ingredients.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Ingredient> AddItem(Ingredient item)
        {
            ctx.Ingredients.Add(item);
            await ctx.Save();
            return item;
        }

        public async Task<Ingredient> UpdateItem(int id, Ingredient ingredient)
        {
            var i = await ctx.Ingredients.FirstOrDefaultAsync(x => x.Id == id);
            i.Name = ingredient.Name;
            await ctx.Save();
            return i;
        }

        public async Task DeleteItem(int id)
        {
            var ingredient = await ctx.Ingredients.FirstOrDefaultAsync(x => x.Id == id);
            ctx.Ingredients.Remove(ingredient);
            await ctx.Save();
        }
    }
}

