using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    
        public Ingredient AddItem(Ingredient item)
        {
            ctx.Ingredients.Add(item);
            ctx.Save();
            return item;
        }
    
        public void DeleteItem(int id)
        {
            var ingredient = ctx.Ingredients.FirstOrDefault(x => x.Id == id);
            ctx.Ingredients.Remove(ingredient);
            ctx.Save();
        }
    
        public List<Ingredient> GetAll()
        {
            return ctx.Ingredients.ToList();
        }
    
        public Ingredient GetById(int id)
        {
            return ctx.Ingredients.FirstOrDefault(x => x.Id == id);
        }
    
        public Ingredient UpdateItem(int id, Ingredient ingredient)
        {
            var i = ctx.Ingredients.FirstOrDefault(x => x.Id == id);
            i.Name = ingredient.Name;
            i.Id = id;
            ctx.Save();
            return i;
        }
    }
}

