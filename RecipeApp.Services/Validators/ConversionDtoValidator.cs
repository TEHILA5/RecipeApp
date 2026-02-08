using FluentValidation;
using RecipeApp.Common.DTOs;

namespace RecipeApp.Service.Validators
{
    /// <summary>
    /// Validator for Conversion DTO - handles ingredient conversions
    /// </summary>
    public class ConversionDtoValidator : AbstractValidator<ConversionCreateDto>
    {
        public ConversionDtoValidator()
        {
            // IngredientId1 validation
            RuleFor(x => x.IngredientId1)
                .GreaterThan(0).WithMessage("מזהה מרכיב ראשון חייב להיות גדול מ-0");

            // IngredientId2 validation
            RuleFor(x => x.IngredientId2)
                .GreaterThan(0).WithMessage("מזהה מרכיב שני חייב להיות גדול מ-0");

            // Validate that two ingredients are different
            RuleFor(x => x.IngredientId2)
                .NotEqual(x => x.IngredientId1)
                .WithMessage("לא ניתן להמיר מרכיב לעצמו");

            // ConversionRatio validation
            RuleFor(x => x.ConversionRatio)
                .GreaterThan(0).WithMessage("יחס המרה חייב להיות גדול מ-0")
                .LessThanOrEqualTo(1000).WithMessage("יחס המרה לא יכול להיות יותר מ-1000");

            // IsBidirectional is a boolean, no validation needed
        }
    }
    /// <summary>
    /// Validator for ConversionUpdateDto - handles ingredient conversions updates
    /// </summary>
    public class ConversionUpdateDtoValidator : AbstractValidator<ConversionUpdateDto>
    {
        public ConversionUpdateDtoValidator()
        {
            // ConversionRatio validation - רק אם סופק ערך
            RuleFor(x => x.ConversionRatio)
                .GreaterThan(0).WithMessage("יחס המרה חייב להיות גדול מ-0")
                .LessThanOrEqualTo(1000).WithMessage("יחס המרה לא יכול להיות יותר מ-1000")
                .When(x => x.ConversionRatio.HasValue);

            // IsBidirectional is nullable boolean, no special validation needed
        }
    }
}