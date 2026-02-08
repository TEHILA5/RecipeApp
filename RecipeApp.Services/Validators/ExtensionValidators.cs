using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RecipeApp.Service.Validators;
using RecipeApp.Services.Interfaces;
using RecipeApp.Services.Services;
using FluentValidation;
using RecipeApp.Common.DTOs;

namespace RecipeApp.Services.Validators
{
    public static class ExtensionValidators
    {
        public static IServiceCollection AddValidations(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<UserDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<UserActionDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<ConversionDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<ConversionUpdateDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<IngredientDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<IngredientUpdateDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<RecipeCreateDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<RecipeUpdateDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<RecipeIngredientCreateDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<BookCreateDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<CommentCreateDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<HistoryCreateDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<UserUpdateDtoValidator>();
            return services;
        }
    }
}
