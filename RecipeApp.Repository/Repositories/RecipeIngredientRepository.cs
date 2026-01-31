using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public RecipeIngredient AddItem(RecipeIngredient item)
        {
            ctx.RecipeIngredients.Add(item);
            ctx.Save();
            return item;
        }

        public void DeleteItem(int id)
        {
            // RecipeIngredient אין לו Id יחיד - צריך RecipeId + IngredientId
            // זו דוגמה - צריך להתאים לצורך
            var ri = ctx.RecipeIngredients.FirstOrDefault(x => x.RecipeId == id);
            ctx.RecipeIngredients.Remove(ri);
            ctx.Save();
        }

        public List<RecipeIngredient> GetAll()
        {
            return ctx.RecipeIngredients.ToList();
        }

        public RecipeIngredient GetById(int id)
        {
            // RecipeIngredient אין לו Id יחיד
            return ctx.RecipeIngredients.FirstOrDefault(x => x.RecipeId == id);
        }

        public RecipeIngredient UpdateItem(int id, RecipeIngredient recipeIngredient)
        {
            var ri = ctx.RecipeIngredients.FirstOrDefault(x =>
                x.RecipeId == recipeIngredient.RecipeId &&
                x.IngredientId == recipeIngredient.IngredientId);

            ri.Quantity = recipeIngredient.Quantity;
            ri.Unit = recipeIngredient.Unit;
            ri.Importance = recipeIngredient.Importance;
            ctx.Save();
            return ri;
        }
    }
}
