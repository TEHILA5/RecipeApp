using FluentValidation;
using RecipeApp.Common.DTOs;

namespace RecipeApp.Service.Validators
{ 
    public class RecipeCreateDtoValidator : AbstractValidator<RecipeCreateDto>
    {
        public RecipeCreateDtoValidator()
        { 
            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Recipe name is required")
                .MinimumLength(2).WithMessage("Recipe name must be at least 2 characters")
                .MaximumLength(200).WithMessage("Recipe name can be up to 200 characters");
             
            RuleFor(x => x.Description)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Recipe description is required")
                .MaximumLength(1000).WithMessage("Description can be up to 1000 characters");
             
            RuleFor(x => x.Category)
                .Cascade(CascadeMode.Stop)
                .IsInEnum().WithMessage("Invalid category");
             
            RuleFor(x => x.Instructions)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Instructions are required")
                .MinimumLength(10).WithMessage("Instructions must be at least 10 characters");
             
            RuleFor(x => x.ArrImage)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Image is required")
                .MaximumLength(500).WithMessage("Image path is too long");
             
            RuleFor(x => x.Servings)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Servings must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Servings cannot exceed 100");
             
            RuleFor(x => x.Level)
                .Cascade(CascadeMode.Stop)
                .InclusiveBetween(1, 5).WithMessage("Difficulty level must be between 1 and 5");
             
            RuleFor(x => x.PrepTime)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Preparation time must be greater than 0")
                .LessThanOrEqualTo(1440).WithMessage("Preparation time cannot exceed 24 hours (1440 minutes)");
             
            RuleFor(x => x.TotalTime)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Total time must be greater than 0")
                .LessThanOrEqualTo(2880).WithMessage("Total time cannot exceed 48 hours (2880 minutes)");
             
            RuleFor(x => x.TotalTime)
                .Cascade(CascadeMode.Stop)
                .GreaterThanOrEqualTo(x => x.PrepTime)
                .WithMessage("Total time must be greater than or equal to preparation time");
             
            RuleFor(x => x.Ingredients)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Recipe must have at least one ingredient")
                .Must(x => x != null && x.Count <= 50).WithMessage("Recipe cannot have more than 50 ingredients");
             
            RuleForEach(x => x.Ingredients)
                .SetValidator(new RecipeIngredientCreateDtoValidator())
                .When(x => x.Ingredients != null);
        }
    } 
     
        public class RecipeUpdateDtoValidator : AbstractValidator<RecipeUpdateDto>
        {
            public RecipeUpdateDtoValidator()
            {
            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .MinimumLength(2).WithMessage("Recipe name must be at least 2 characters")
                .MaximumLength(200).WithMessage("Recipe name can be up to 200 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Name));

            RuleFor(x => x.Description)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(1000).WithMessage("Description can be up to 1000 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.Category)
                .Cascade(CascadeMode.Stop)
                .IsInEnum().WithMessage("Invalid category")
                .When(x => x.Category.HasValue);

            RuleFor(x => x.Instructions)
                .Cascade(CascadeMode.Stop)
                .MinimumLength(10).WithMessage("Instructions must be at least 10 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Instructions));

            RuleFor(x => x.ArrImage)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(500).WithMessage("Image path is too long")
                .When(x => !string.IsNullOrWhiteSpace(x.ArrImage));

            RuleFor(x => x.Servings)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Servings must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Servings cannot exceed 100")
                .When(x => x.Servings.HasValue);

            RuleFor(x => x.Level)
                .Cascade(CascadeMode.Stop)
                .InclusiveBetween(1, 5).WithMessage("Difficulty level must be between 1 and 5")
                .When(x => x.Level.HasValue);

            RuleFor(x => x.PrepTime)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Preparation time must be greater than 0")
                .LessThanOrEqualTo(1440).WithMessage("Preparation time cannot exceed 24 hours (1440 minutes)")
                .When(x => x.PrepTime.HasValue);

            RuleFor(x => x.TotalTime)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Total time must be greater than 0")
                .LessThanOrEqualTo(2880).WithMessage("Total time cannot exceed 48 hours (2880 minutes)")
                .When(x => x.TotalTime.HasValue);

            RuleFor(x => x.TotalTime)
                .Cascade(CascadeMode.Stop)
                .GreaterThanOrEqualTo(x => x.PrepTime)
                .WithMessage("Total time must be greater than or equal to preparation time")
                .When(x => x.TotalTime.HasValue && x.PrepTime.HasValue);

            RuleFor(x => x.Ingredients)
                .Cascade(CascadeMode.Stop)
                .Must(x => x != null && x.Count > 0).WithMessage("If updating ingredients, there must be at least one")
                .Must(x => x.Count <= 50).WithMessage("Recipe cannot have more than 50 ingredients")
                .When(x => x.Ingredients != null);

            RuleForEach(x => x.Ingredients)
                .SetValidator(new RecipeIngredientCreateDtoValidator())
                .When(x => x.Ingredients != null);
        }
    }
   
}