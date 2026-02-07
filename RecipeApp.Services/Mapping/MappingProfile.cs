using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using RecipeApp.Common.DTOs;
using RecipeApp.Repository.Entities;

namespace RecipeApp.Services.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User 

            // Entity ↔ AdminDto (כולל Id, Email, Phone)
            CreateMap<User, UserAdminDto>().ReverseMap();

            // Entity → UserDto  
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

            // CreateDto → Entity
            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UserActions, opt => opt.Ignore());

            // UpdateDto → Entity  
            CreateMap<UserUpdateDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UserActions, opt => opt.Ignore());

            // AdminUpdateDto → Entity
            CreateMap<UserAdminUpdateDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UserActions, opt => opt.Ignore());

            // Recipe
            CreateMap<Recipe, RecipeDto>()
                .ForMember(dest => dest.ArrImage, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.ImageUrl) ? null : Convert.FromBase64String(src.ImageUrl)))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Ingredients, opt => opt.Ignore())
                .ForMember(dest => dest.AverageRating, opt => opt.Ignore())
                .ForMember(dest => dest.CommentCount, opt => opt.Ignore());

            CreateMap<RecipeDto, Recipe>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    src.ArrImage == null ? null : Convert.ToBase64String(src.ArrImage)))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => (src.Category)))
                .ForMember(dest => dest.RecipeIngredients, opt => opt.Ignore())
                .ForMember(dest => dest.UserActions, opt => opt.Ignore());

            // CreateDto → Entity
            CreateMap<RecipeCreateDto, Recipe>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    src.ArrImage == null ? null : Convert.ToBase64String(src.ArrImage)))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.RecipeIngredients, opt => opt.Ignore())
                .ForMember(dest => dest.UserActions, opt => opt.Ignore());

            // Ingredient
            CreateMap<Ingredient, IngredientDto>().ReverseMap();

            CreateMap<IngredientDto, Ingredient>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());  

            CreateMap<IngredientCreateDto, Ingredient>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RecipeIngredients, opt => opt.Ignore())
                .ForMember(dest => dest.ConversionsFrom, opt => opt.Ignore())
                .ForMember(dest => dest.ConversionsTo, opt => opt.Ignore());

            // RecipeIngredient
            CreateMap<RecipeIngredient, RecipeIngredientDto>()
                .ForMember(dest => dest.IngredientName, opt => opt.MapFrom(src => src.Ingredient.Name))
                .ForMember(dest => dest.Importance, opt => opt.MapFrom(src => src.Importance.ToString()));

            CreateMap<RecipeIngredientCreateDto, RecipeIngredient>()
                .ForMember(dest => dest.Recipe, opt => opt.Ignore())
                .ForMember(dest => dest.Ingredient, opt => opt.Ignore())
                .ForMember(dest => dest.Importance, opt => opt.MapFrom(src =>
                    Enum.Parse<IngredientImportance>(src.Importance)));

            // Conversion
            CreateMap<Conversion, ConversionDto>()
                .ForMember(dest => dest.Ingredient1Name, opt => opt.MapFrom(src => src.Ingredient1.Name))
                .ForMember(dest => dest.Ingredient2Name, opt => opt.MapFrom(src => src.Ingredient2.Name));

            CreateMap<ConversionDto, Conversion>()
                 .ForMember(dest => dest.Id, opt => opt.Ignore());  

            CreateMap<ConversionCreateDto, Conversion>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Ingredient1, opt => opt.Ignore())
                .ForMember(dest => dest.Ingredient2, opt => opt.Ignore());

            CreateMap<ConversionDto, Conversion>()
                .ForMember(dest => dest.IngredientId1, opt => opt.Ignore())
                .ForMember(dest => dest.IngredientId2, opt => opt.Ignore())
                .ForMember(dest => dest.Ingredient1, opt => opt.Ignore())
                .ForMember(dest => dest.Ingredient2, opt => opt.Ignore());

            // UserAction
            CreateMap<UserAction, UserActionDto>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src =>
                    src.Category.HasValue ? src.Category.Value.ToString() : null))
                .ForMember(dest => dest.RecipeName, opt => opt.Ignore())
                .ForMember(dest => dest.RecipeImageUrl, opt => opt.Ignore());

            CreateMap<UserActionDto, UserAction>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Recipe, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.Category.ToString() ?? RecipeCategory.Sweats.ToString()) ? 
                    src.Category : (RecipeCategory?)null));

            // Create DTOs
            CreateMap<CommentCreateDto, UserAction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.ActionType, opt => opt.MapFrom(src => UserActionType.Comment))
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Recipe, opt => opt.Ignore());

            CreateMap<BookCreateDto, UserAction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.ActionType, opt => opt.MapFrom(src => UserActionType.Book))
                .ForMember(dest => dest.Content, opt => opt.Ignore())
                .ForMember(dest => dest.Rating, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Recipe, opt => opt.Ignore());

            CreateMap<HistoryCreateDto, UserAction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.ActionType, opt => opt.MapFrom(src => UserActionType.History))
                .ForMember(dest => dest.RecipeId, opt => opt.Ignore())
                .ForMember(dest => dest.Content, opt => opt.Ignore())
                .ForMember(dest => dest.Rating, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src =>src.Category))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Recipe, opt => opt.Ignore());
        }
    }
}
