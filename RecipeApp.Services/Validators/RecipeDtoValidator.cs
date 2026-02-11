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
                .NotEmpty().WithMessage("שם המתכון הוא שדה חובה")
                .MinimumLength(2).WithMessage("שם המתכון חייב להכיל לפחות 2 תווים")
                .MaximumLength(200).WithMessage("שם המתכון יכול להכיל עד 200 תווים");

            // Description validation
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("תיאור המתכון הוא שדה חובה")
                .MaximumLength(1000).WithMessage("תיאור יכול להכיל עד 1000 תווים");

            // Category validation
            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("קטגוריה לא חוקית");

            // Instructions validation
            RuleFor(x => x.Instructions)
                .NotEmpty().WithMessage("הוראות הכנה הן שדה חובה")
                .MinimumLength(10).WithMessage("הוראות הכנה חייבות להכיל לפחות 10 תווים");

            // Image validation
            RuleFor(x => x.ImageUrl)
                .NotEmpty().WithMessage("חובה לציין תמונה")
                .MaximumLength(500).WithMessage("נתיב התמונה ארוך מדי");

            // Servings validation
            RuleFor(x => x.Servings)
                .GreaterThan(0).WithMessage("מספר מנות חייב להיות גדול מ-0")
                .LessThanOrEqualTo(100).WithMessage("מספר מנות לא יכול להיות יותר מ-100");

            // Level validation (1-5)
            RuleFor(x => x.Level)
                .InclusiveBetween(1, 5).WithMessage("רמת קושי חייבת להיות בין 1 ל-5");

            // PrepTime validation
            RuleFor(x => x.PrepTime)
                .GreaterThan(0).WithMessage("זמן הכנה חייב להיות גדול מ-0")
                .LessThanOrEqualTo(1440).WithMessage("זמן הכנה לא יכול להיות יותר מ-24 שעות (1440 דקות)");

            // TotalTime validation
            RuleFor(x => x.TotalTime)
                .GreaterThan(0).WithMessage("זמן כולל חייב להיות גדול מ-0")
                .LessThanOrEqualTo(2880).WithMessage("זמן כולל לא יכול להיות יותר מ-48 שעות (2880 דקות)");

            // Validate that TotalTime >= PrepTime
            RuleFor(x => x.TotalTime)
                .GreaterThanOrEqualTo(x => x.PrepTime)
                .WithMessage("זמן כולל חייב להיות גדול או שווה לזמן ההכנה");

            // Ingredients validation
            RuleFor(x => x.Ingredients)
                .NotEmpty().WithMessage("מתכון חייב להכיל לפחות מרכיב אחד")
                .Must(x => x != null && x.Count <= 50).WithMessage("מתכון לא יכול להכיל יותר מ-50 מרכיבים");

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
                    .MinimumLength(2).WithMessage("שם המתכון חייב להכיל לפחות 2 תווים")
                    .MaximumLength(200).WithMessage("שם המתכון יכול להכיל עד 200 תווים")
                    .When(x => !string.IsNullOrWhiteSpace(x.Name));

                // Description validation - optional but if provided must be valid
                RuleFor(x => x.Description)
                    .MaximumLength(1000).WithMessage("תיאור יכול להכיל עד 1000 תווים")
                    .When(x => !string.IsNullOrWhiteSpace(x.Description));

                // Category validation - optional but if provided must be valid
                RuleFor(x => x.Category)
                    .IsInEnum().WithMessage("קטגוריה לא חוקית")
                    .When(x => x.Category.HasValue);

                // Instructions validation - optional but if provided must be valid
                RuleFor(x => x.Instructions)
                    .MinimumLength(10).WithMessage("הוראות הכנה חייבות להכיל לפחות 10 תווים")
                    .When(x => !string.IsNullOrWhiteSpace(x.Instructions));

                // Image validation - optional but if provided must be valid
                RuleFor(x => x.ImageUrl)
                    .MaximumLength(500).WithMessage("נתיב התמונה ארוך מדי")
                    .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

                // Servings validation - optional but if provided must be valid
                RuleFor(x => x.Servings)
                    .GreaterThan(0).WithMessage("מספר מנות חייב להיות גדול מ-0")
                    .LessThanOrEqualTo(100).WithMessage("מספר מנות לא יכול להיות יותר מ-100")
                    .When(x => x.Servings.HasValue);

                // Level validation - optional but if provided must be valid
                RuleFor(x => x.Level)
                    .InclusiveBetween(1, 5).WithMessage("רמת קושי חייבת להיות בין 1 ל-5")
                    .When(x => x.Level.HasValue);

                // PrepTime validation - optional but if provided must be valid
                RuleFor(x => x.PrepTime)
                    .GreaterThan(0).WithMessage("זמן הכנה חייב להיות גדול מ-0")
                    .LessThanOrEqualTo(1440).WithMessage("זמן הכנה לא יכול להיות יותר מ-24 שעות (1440 דקות)")
                    .When(x => x.PrepTime.HasValue);

                // TotalTime validation - optional but if provided must be valid
                RuleFor(x => x.TotalTime)
                    .GreaterThan(0).WithMessage("זמן כולל חייב להיות גדול מ-0")
                    .LessThanOrEqualTo(2880).WithMessage("זמן כולל לא יכול להיות יותר מ-48 שעות (2880 דקות)")
                    .When(x => x.TotalTime.HasValue);

                // Validate that TotalTime >= PrepTime (only if both provided)
                RuleFor(x => x.TotalTime)
                    .GreaterThanOrEqualTo(x => x.PrepTime)
                    .WithMessage("זמן כולל חייב להיות גדול או שווה לזמן ההכנה")
                    .When(x => x.TotalTime.HasValue && x.PrepTime.HasValue);

                // Ingredients validation - optional but if provided must be valid
                RuleFor(x => x.Ingredients)
                    .Must(x => x != null && x.Count > 0).WithMessage("אם מעדכנים מצרכים, חייב להיות לפחות מרכיב אחד")
                    .Must(x => x.Count <= 50).WithMessage("מתכון לא יכול להכיל יותר מ-50 מרכיבים")
                    .When(x => x.Ingredients != null);

                // Validate each ingredient if provided
                RuleForEach(x => x.Ingredients)
                    .SetValidator(new RecipeIngredientCreateDtoValidator())
                    .When(x => x.Ingredients != null);
            }
        }
   
}