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
                .GreaterThan(0).WithMessage("First ingredient ID must be greater than 0");

            // IngredientId2 validation
            RuleFor(x => x.IngredientId2)
                .GreaterThan(0).WithMessage("Second ingredient ID must be greater than 0");

            // Validate that two ingredients are different
            RuleFor(x => x.IngredientId2)
                .NotEqual(x => x.IngredientId1)
                .WithMessage("Cannot convert an ingredient to itself");

            // ConversionRatio validation
            RuleFor(x => x.ConversionRatio)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Conversion ratio must be greater than 0")
                .LessThanOrEqualTo(1000).WithMessage("Conversion ratio cannot be greater than 1000");

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
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0)
            .WithMessage("Conversion ratio must be greater than 0")
            .LessThanOrEqualTo(1000)
            .WithMessage("Conversion ratio cannot be greater than 1000")
            .When(x => x.ConversionRatio.HasValue);

            // IsBidirectional is nullable boolean, no special validation needed
        }
    }
}