using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecipeApp.Repository.Entities;

namespace RecipeApp.Repository.Interfaces
{
    public interface IContext
    {
        public DbSet<UserAction> UserActions { get; set; } 
        public DbSet<Conversion> Conversions { get; set; } 
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<User> Users { get; set; }
        public void Save();

    }
}
