using FluentValidation;
using RecipeApp.Common.DTOs;

namespace RecipeApp.Service.Validators
{
    /// <summary>
    /// Validator for RecipeIngredientCreateDto
    /// </summary>
    public class RecipeIngredientCreateDtoValidator : AbstractValidator<RecipeIngredientCreateDto>
    {
        public RecipeIngredientCreateDtoValidator()
        {
            // IngredientId validation
            RuleFor(x => x.IngredientId)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Ingredient ID must be greater than 0");

            // Quantity validation
            RuleFor(x => x.Quantity)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(10000).WithMessage("Quantity cannot exceed 10000");

            // Unit validation
            RuleFor(x => x.Unit)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Unit is required")
                .MaximumLength(50).WithMessage("Unit can be up to 50 characters");

            // Importance validation (if exists in your DTO)
            RuleFor(x => x.Importance)
                .Cascade(CascadeMode.Stop)
                .IsInEnum().WithMessage("Invalid importance level")
                .When(x => x.Importance != 0); // if Importance exists
        }
    }
}