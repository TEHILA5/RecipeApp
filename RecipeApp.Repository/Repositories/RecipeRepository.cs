using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public Recipe AddItem(Recipe item)
        {
            ctx.Recipes.Add(item);
            ctx.Save();
            return item;
        }

        public void DeleteItem(int id)
        {
            var recipe = ctx.Recipes.FirstOrDefault(x => x.Id == id);
            ctx.Recipes.Remove(recipe);
            ctx.Save();
        }

        public List<Recipe> GetAll()
        {
            return ctx.Recipes.ToList();
        }

        public Recipe GetById(int id)
        {
            return ctx.Recipes.FirstOrDefault(x => x.Id == id);
        }

        public Recipe UpdateItem(int id, Recipe recipe)
        {
            var r = ctx.Recipes.FirstOrDefault(x => x.Id == id);
            r.Name = recipe.Name;
            r.Description = recipe.Description;
            r.Category = recipe.Category;
            r.Instructions = recipe.Instructions;
            r.ImageUrl = recipe.ImageUrl;
            r.Servings = recipe.Servings;
            r.Level = recipe.Level;
            r.PrepTime = recipe.PrepTime;
            r.TotalTime = recipe.TotalTime;
            r.Id = id;
            ctx.Save();
            return r;
        }
    }
}
