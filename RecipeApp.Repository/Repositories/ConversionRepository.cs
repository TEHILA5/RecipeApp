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
    public class ConversionRepository : IRepository<Conversion>
    {
        private readonly IContext ctx;

        public ConversionRepository(IContext context)
        {
            ctx = context;
        }

        public async Task<List<Conversion>> GetAll()
        {
            return await ctx.Conversions.ToListAsync();
        }

        public async Task<Conversion> GetById(int id)
        {
            return await ctx.Conversions.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Conversion> AddItem(Conversion item)
        {
            ctx.Conversions.Add(item);
            await ctx.Save();
            return item;
        }

        public async Task<Conversion> UpdateItem(int id, Conversion conversion)
        {
            var c = await ctx.Conversions.FirstOrDefaultAsync(x => x.Id == id);
            c.IngredientId1 = conversion.IngredientId1;
            c.IngredientId2 = conversion.IngredientId2;
            c.ConversionRatio = conversion.ConversionRatio;
            c.IsBidirectional = conversion.IsBidirectional;
            await ctx.Save();
            return c;
        }

        public async Task DeleteItem(int id)
        {
            var conversion = await ctx.Conversions.FirstOrDefaultAsync(x => x.Id == id);
            ctx.Conversions.Remove(conversion);
            await ctx.Save();
        }
    }
}
