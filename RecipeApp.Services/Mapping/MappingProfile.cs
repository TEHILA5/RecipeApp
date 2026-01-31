using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using RecipeApp.Common.DTOs;
using RecipeApp.Repository.Entities;

namespace RecipeApp.Services.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<UserCreateDto, User>();
            CreateMap<UserLoginDto, User>();

            // Recipe
            CreateMap<Recipe, RecipeDto>().ReverseMap();

            // Ingredient
            CreateMap<Ingredient, IngredientDto>().ReverseMap();

            // RecipeIngredient
            CreateMap<RecipeIngredient, RecipeIngredientDto>().ReverseMap();

            // Conversion
            CreateMap<Conversion, ConversionDto>().ReverseMap();

            // UserAction
            CreateMap<UserAction, UserActionDto>().ReverseMap();
        }
    }
}
