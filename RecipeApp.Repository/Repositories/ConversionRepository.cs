using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipeApp.Repository.Interfaces;
using RecipeApp.Repository.Entities;

namespace RecipeApp.Repository.Repositories
{
    public class ConversionRepository : IRepository<Conversion>
    {
        private readonly IContext ctx;

        public ConversionRepository(IContext context)
        {
            ctx = context;
        }

        public Conversion AddItem(Conversion item)
        {
            ctx.Conversions.Add(item);
            ctx.Save();
            return item;
        }

        public void DeleteItem(int id)
        {
            var conversion = ctx.Conversions.FirstOrDefault(x => x.Id == id);
            ctx.Conversions.Remove(conversion);
            ctx.Save();
        }

        public List<Conversion> GetAll()
        {
            return ctx.Conversions.ToList();
        }

        public Conversion GetById(int id)
        {
            return ctx.Conversions.FirstOrDefault(x => x.Id == id);
        }

        public Conversion UpdateItem(int id, Conversion conversion)
        {
            var c = ctx.Conversions.FirstOrDefault(x => x.Id == id);
            c.IngredientId1 = conversion.IngredientId1;
            c.IngredientId2 = conversion.IngredientId2;
            c.ConversionRatio = conversion.ConversionRatio;
            c.IsBidirectional = conversion.IsBidirectional;
            c.Id = id;
            ctx.Save();
            return c;
        }
    }
}
