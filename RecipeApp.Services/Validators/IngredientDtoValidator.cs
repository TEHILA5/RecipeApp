using FluentValidation;
using RecipeApp.Common.DTOs;

namespace RecipeApp.Service.Validators
{
    /// <summary>
    /// Validator for Ingredient DTO
    /// </summary>
    public class IngredientDtoValidator : AbstractValidator<IngredientCreateDto>
    {
        public IngredientDtoValidator()
        {
            // Name validation
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("שם המרכיב הוא שדה חובה")
                .MinimumLength(2).WithMessage("שם המרכיב חייב להכיל לפחות 2 תווים")
                .MaximumLength(100).WithMessage("שם המרכיב יכול להכיל עד 100 תווים")
                .Matches(@"^[\u0590-\u05FFa-zA-Z\s\-'""]+$")
                .WithMessage("שם המרכיב יכול להכיל רק אותיות, רווחים, מקפים ומרכאות");
        }
    }
    public class IngredientUpdateDtoValidator : AbstractValidator<IngredientUpdateDto>
    {
        public IngredientUpdateDtoValidator()
        {
            // Name validation
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("שם המרכיב הוא שדה חובה")
                .MinimumLength(2).WithMessage("שם המרכיב חייב להכיל לפחות 2 תווים")
                .MaximumLength(100).WithMessage("שם המרכיב יכול להכיל עד 100 תווים")
                .Matches(@"^[\u0590-\u05FFa-zA-Z\s\-'""]+$")
                .WithMessage("שם המרכיב יכול להכיל רק אותיות, רווחים, מקפים ומרכאות");
        }
    }
}