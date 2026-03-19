using FluentValidation;
using RecipeApp.Common.DTOs;

namespace RecipeApp.Service.Validators
{ 
    public class RecipeIngredientCreateDtoValidator : AbstractValidator<RecipeIngredientCreateDto>
    {
        public RecipeIngredientCreateDtoValidator()
        { 
            RuleFor(x => x.IngredientId)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Ingredient ID must be greater than 0");
             
            RuleFor(x => x.Quantity)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(10000).WithMessage("Quantity cannot exceed 10000");
             
            RuleFor(x => x.Unit)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Unit is required")
                .MaximumLength(50).WithMessage("Unit can be up to 50 characters");
             
            RuleFor(x => x.Importance)
                .Cascade(CascadeMode.Stop)
                .IsInEnum().WithMessage("Invalid importance level")
                .When(x => x.Importance != 0); 
        }
    }
}