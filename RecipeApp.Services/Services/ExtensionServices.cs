using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RecipeApp.Services.Interfaces;

namespace RecipeApp.Services.Services
{
    public static class ExtensionServices
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRecipeService, RecipeService>();
            services.AddScoped<IIngredientService, IngredientService>();
            services.AddScoped<IUserActionService, UserActionService>();
            services.AddScoped<IConversionService, ConversionService>();
            return services;
        }
    }
}
