using FluentValidation;
using RecipeApp.Common.DTOs;

namespace RecipeApp.Service.Validators
{ 
    public class ConversionDtoValidator : AbstractValidator<ConversionCreateDto>
    {
        public ConversionDtoValidator()
        { 
            RuleFor(x => x.IngredientId1)
                .GreaterThan(0).WithMessage("First ingredient ID must be greater than 0");
             
            RuleFor(x => x.IngredientId2)
                .GreaterThan(0).WithMessage("Second ingredient ID must be greater than 0");
             
            RuleFor(x => x.IngredientId2)
                .NotEqual(x => x.IngredientId1)
                .WithMessage("Cannot convert an ingredient to itself");
             
            RuleFor(x => x.ConversionRatio)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Conversion ratio must be greater than 0")
                .LessThanOrEqualTo(1000).WithMessage("Conversion ratio cannot be greater than 1000");
             
        }
    } 
    public class ConversionUpdateDtoValidator : AbstractValidator<ConversionUpdateDto>
    {
        public ConversionUpdateDtoValidator()
        { 
            RuleFor(x => x.ConversionRatio)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0)
            .WithMessage("Conversion ratio must be greater than 0")
            .LessThanOrEqualTo(1000)
            .WithMessage("Conversion ratio cannot be greater than 1000")
            .When(x => x.ConversionRatio.HasValue);

        }
    }
}