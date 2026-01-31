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
    public class RecipeIngredientRepository : IRepository<RecipeIngredient>
    {
        private readonly IContext ctx;

        public RecipeIngredientRepository(IContext context)
        {
            ctx = context;
        }

        public async Task<List<RecipeIngredient>> GetAll()
        {
            return await ctx.RecipeIngredients.ToListAsync();
        }

        public async Task<RecipeIngredient> GetById(int id)
        {
            // RecipeIngredient אין לו Id יחיד — חיפוש לפי RecipeId
            return await ctx.RecipeIngredients.FirstOrDefaultAsync(x => x.RecipeId == id);
        }

        public async Task<RecipeIngredient> AddItem(RecipeIngredient item)
        {
            ctx.RecipeIngredients.Add(item);
            await ctx.Save();
            return item;
        }

        public async Task<RecipeIngredient> UpdateItem(int id, RecipeIngredient recipeIngredient)
        {
            var ri = await ctx.RecipeIngredients.FirstOrDefaultAsync(x =>
                x.RecipeId == recipeIngredient.RecipeId &&
                x.IngredientId == recipeIngredient.IngredientId);
            ri.Quantity = recipeIngredient.Quantity;
            ri.Unit = recipeIngredient.Unit;
            ri.Importance = recipeIngredient.Importance;
            await ctx.Save();
            return ri;
        }

        public async Task DeleteItem(int id)
        {
            // RecipeIngredient אין לו Id יחיד — מחיקה לפי RecipeId
            var ri = await ctx.RecipeIngredients.FirstOrDefaultAsync(x => x.RecipeId == id);
            ctx.RecipeIngredients.Remove(ri);
            await ctx.Save();
        }
    }
}
