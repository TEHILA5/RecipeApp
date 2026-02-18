using FluentValidation;
using RecipeApp.Common.DTOs;

namespace RecipeApp.Service.Validators
{
    /// <summary>
    /// Validator for RecipeCreateDto - handles recipe creation
    /// </summary>
    public class RecipeCreateDtoValidator : AbstractValidator<RecipeCreateDto>
    {
        public RecipeCreateDtoValidator()
        {
            // Name validation
            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Recipe name is required")
                .MinimumLength(2).WithMessage("Recipe name must be at least 2 characters")
                .MaximumLength(200).WithMessage("Recipe name can be up to 200 characters");

            // Description validation
            RuleFor(x => x.Description)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Recipe description is required")
                .MaximumLength(1000).WithMessage("Description can be up to 1000 characters");

            // Category validation
            RuleFor(x => x.Category)
                .Cascade(CascadeMode.Stop)
                .IsInEnum().WithMessage("Invalid category");

            // Instructions validation
            RuleFor(x => x.Instructions)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Instructions are required")
                .MinimumLength(10).WithMessage("Instructions must be at least 10 characters");

            // Image validation
            RuleFor(x => x.ArrImage)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Image is required")
                .MaximumLength(500).WithMessage("Image path is too long");

            // Servings validation
            RuleFor(x => x.Servings)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Servings must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Servings cannot exceed 100");

            // Level validation (1-5)
            RuleFor(x => x.Level)
                .Cascade(CascadeMode.Stop)
                .InclusiveBetween(1, 5).WithMessage("Difficulty level must be between 1 and 5");

            // PrepTime validation
            RuleFor(x => x.PrepTime)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Preparation time must be greater than 0")
                .LessThanOrEqualTo(1440).WithMessage("Preparation time cannot exceed 24 hours (1440 minutes)");

            // TotalTime validation
            RuleFor(x => x.TotalTime)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Total time must be greater than 0")
                .LessThanOrEqualTo(2880).WithMessage("Total time cannot exceed 48 hours (2880 minutes)");

            // Validate that TotalTime >= PrepTime
            RuleFor(x => x.TotalTime)
                .Cascade(CascadeMode.Stop)
                .GreaterThanOrEqualTo(x => x.PrepTime)
                .WithMessage("Total time must be greater than or equal to preparation time");

            // Ingredients validation
            RuleFor(x => x.Ingredients)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Recipe must have at least one ingredient")
                .Must(x => x != null && x.Count <= 50).WithMessage("Recipe cannot have more than 50 ingredients");

            // Validate each ingredient
            RuleForEach(x => x.Ingredients)
                .SetValidator(new RecipeIngredientCreateDtoValidator())
                .When(x => x.Ingredients != null);
        }
    } 

        /// <summary>
        /// Validator for RecipeUpdateDto - handles recipe updates (partial)
        /// </summary>
        public class RecipeUpdateDtoValidator : AbstractValidator<RecipeUpdateDto>
        {
            public RecipeUpdateDtoValidator()
            {
            // Name validation - optional but if provided must be valid
            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .MinimumLength(2).WithMessage("Recipe name must be at least 2 characters")
                .MaximumLength(200).WithMessage("Recipe name can be up to 200 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Name));

            // Description validation - optional but if provided must be valid
            RuleFor(x => x.Description)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(1000).WithMessage("Description can be up to 1000 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            // Category validation - optional but if provided must be valid
            RuleFor(x => x.Category)
                .Cascade(CascadeMode.Stop)
                .IsInEnum().WithMessage("Invalid category")
                .When(x => x.Category.HasValue);

            // Instructions validation - optional but if provided must be valid
            RuleFor(x => x.Instructions)
                .Cascade(CascadeMode.Stop)
                .MinimumLength(10).WithMessage("Instructions must be at least 10 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Instructions));

            // Image validation - optional but if provided must be valid
            RuleFor(x => x.ArrImage)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(500).WithMessage("Image path is too long")
                .When(x => !string.IsNullOrWhiteSpace(x.ArrImage));

            // Servings validation - optional but if provided must be valid
            RuleFor(x => x.Servings)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Servings must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Servings cannot exceed 100")
                .When(x => x.Servings.HasValue);

            // Level validation - optional but if provided must be valid
            RuleFor(x => x.Level)
                .Cascade(CascadeMode.Stop)
                .InclusiveBetween(1, 5).WithMessage("Difficulty level must be between 1 and 5")
                .When(x => x.Level.HasValue);

            // PrepTime validation - optional but if provided must be valid
            RuleFor(x => x.PrepTime)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Preparation time must be greater than 0")
                .LessThanOrEqualTo(1440).WithMessage("Preparation time cannot exceed 24 hours (1440 minutes)")
                .When(x => x.PrepTime.HasValue);

            // TotalTime validation - optional but if provided must be valid
            RuleFor(x => x.TotalTime)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Total time must be greater than 0")
                .LessThanOrEqualTo(2880).WithMessage("Total time cannot exceed 48 hours (2880 minutes)")
                .When(x => x.TotalTime.HasValue);

            // Validate that TotalTime >= PrepTime (only if both provided)
            RuleFor(x => x.TotalTime)
                .Cascade(CascadeMode.Stop)
                .GreaterThanOrEqualTo(x => x.PrepTime)
                .WithMessage("Total time must be greater than or equal to preparation time")
                .When(x => x.TotalTime.HasValue && x.PrepTime.HasValue);

            // Ingredients validation - optional but if provided must be valid
            RuleFor(x => x.Ingredients)
                .Cascade(CascadeMode.Stop)
                .Must(x => x != null && x.Count > 0).WithMessage("If updating ingredients, there must be at least one")
                .Must(x => x.Count <= 50).WithMessage("Recipe cannot have more than 50 ingredients")
                .When(x => x.Ingredients != null);

            // Validate each ingredient if provided
            RuleForEach(x => x.Ingredients)
                .SetValidator(new RecipeIngredientCreateDtoValidator())
                .When(x => x.Ingredients != null);
        }
    }
   
}