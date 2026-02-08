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
                .GreaterThan(0).WithMessage("מזהה מצרך חייב להיות גדול מ-0");

            // Quantity validation
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("כמות חייבת להיות גדולה מ-0")
                .LessThanOrEqualTo(10000).WithMessage("כמות לא יכולה להיות יותר מ-10000");

            // Unit validation
            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage("יחידת מידה היא שדה חובה")
                .MaximumLength(50).WithMessage("יחידת מידה יכולה להכיל עד 50 תווים");

            // Importance validation (if exists in your DTO)
            RuleFor(x => x.Importance)
                .IsInEnum().WithMessage("רמת חשיבות לא חוקית")
                .When(x => x.Importance != 0); // אם יש שדה Importance
        }
    }
}