using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RecipeApp.Repository.Entities;
using RecipeApp.Repository.Interfaces;

namespace RecipeApp.Repository.Repositories
{
    public static class ExtensionRepository
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IRepository<User>, UserRepository>();
            services.AddScoped<IRepository<Recipe>, RecipeRepository>();
            services.AddScoped<IRepository<Ingredient>, IngredientRepository>();
            services.AddScoped<IRepository<RecipeIngredient>, RecipeIngredientRepository>();
            services.AddScoped<IRepository<Conversion>, ConversionRepository>();
            services.AddScoped<IRepository<UserAction>, UserActionRepository>();

            return services;
        }
    }
}
