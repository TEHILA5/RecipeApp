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
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Ingredient name is required")
                .MinimumLength(2).WithMessage("Ingredient name must be at least 2 characters long")
                .MaximumLength(100).WithMessage("Ingredient name can contain up to 100 characters")
                .Matches(@"^[a-zA-Z\s\-]+$")
                .WithMessage("Ingredient name can contain English letters, spaces, and hyphens only");
        }
    }
    public class IngredientUpdateDtoValidator : AbstractValidator<IngredientUpdateDto>
    {
        public IngredientUpdateDtoValidator()
        {
            // Name validation
            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Ingredient name is required")
                .MinimumLength(2).WithMessage("Ingredient name must be at least 2 characters long")
                .MaximumLength(100).WithMessage("Ingredient name can contain up to 100 characters")
                .Matches(@"^[a-zA-Z\s\-'""]+$")
                .WithMessage("Ingredient name can contain English letters, spaces, hyphens, and quotes only");
        }
    }
}