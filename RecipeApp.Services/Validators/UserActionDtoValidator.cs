using FluentValidation;
using RecipeApp.Common.DTOs;

namespace RecipeApp.Service.Validators
{
    /// <summary>
    /// Validator for UserAction DTO - main user action model
    /// </summary>
    public class UserActionDtoValidator : AbstractValidator<UserActionDto>
    {
        public UserActionDtoValidator()
        {
            // ActionType validation
            RuleFor(x => x.ActionType)
                .IsInEnum().WithMessage("סוג פעולה לא חוקי");

            // RecipeId validation - optional field
            RuleFor(x => x.RecipeId)
                .GreaterThan(0).WithMessage("מזהה מתכון חייב להיות גדול מ-0")
                .When(x => x.RecipeId.HasValue);

            // RecipeName validation - optional field
            RuleFor(x => x.RecipeName)
                .MaximumLength(200).WithMessage("שם מתכון יכול להכיל עד 200 תווים")
                .When(x => !string.IsNullOrWhiteSpace(x.RecipeName));

            // RecipeImageUrl validation - optional field
            RuleFor(x => x.RecipeImageUrl)
                .MaximumLength(500).WithMessage("כתובת תמונה יכולה להכיל עד 500 תווים")
                .When(x => !string.IsNullOrWhiteSpace(x.RecipeImageUrl));

            // Category validation - optional field
            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("קטגוריה לא חוקית")
                .When(x => x.Category.HasValue);

            // UserName validation - optional field
            RuleFor(x => x.UserName)
                .MinimumLength(2).WithMessage("שם משתמש חייב להכיל לפחות 2 תווים")
                .MaximumLength(100).WithMessage("שם משתמש יכול להכיל עד 100 תווים")
                .When(x => !string.IsNullOrWhiteSpace(x.UserName));

            // Content validation for comments - optional field
            RuleFor(x => x.Content)
                .MinimumLength(2).WithMessage("תוכן חייב להכיל לפחות 2 תווים")
                .MaximumLength(2000).WithMessage("תוכן יכול להכיל עד 2000 תווים")
                .When(x => !string.IsNullOrWhiteSpace(x.Content));

            // Rating validation - optional field
            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("דירוג חייב להיות בין 1 ל-5")
                .When(x => x.Rating.HasValue);
        }
    }

    /// <summary>
    /// Validator for BookCreateDto - creating a bookmark
    /// </summary>
    public class BookCreateDtoValidator : AbstractValidator<BookCreateDto>
    {
        public BookCreateDtoValidator()
        {
            RuleFor(x => x.RecipeId)
                .NotEmpty().WithMessage("מזהה מתכון הוא שדה חובה")
                .GreaterThan(0).WithMessage("מזהה מתכון חייב להיות גדול מ-0");
        }
    }

    /// <summary>
    /// Validator for CommentCreateDto - creating a comment with rating
    /// </summary>
    public class CommentCreateDtoValidator : AbstractValidator<CommentCreateDto>
    {
        public CommentCreateDtoValidator()
        {
            // RecipeId is required
            RuleFor(x => x.RecipeId)
                .NotEmpty().WithMessage("מזהה מתכון הוא שדה חובה")
                .GreaterThan(0).WithMessage("מזהה מתכון חייב להיות גדול מ-0");

            // Content is required
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("תוכן התגובה הוא שדה חובה")
                .MinimumLength(2).WithMessage("תגובה חייבת להכיל לפחות 2 תווים")
                .MaximumLength(2000).WithMessage("תגובה יכולה להכיל עד 2000 תווים")
                .Must(NotContainOnlyWhitespace).WithMessage("תגובה לא יכולה להכיל רק רווחים");

            // Rating is required
            RuleFor(x => x.Rating)
                .NotEmpty().WithMessage("דירוג הוא שדה חובה")
                .InclusiveBetween(1, 5).WithMessage("דירוג חייב להיות בין 1 ל-5 כוכבים");
        }

        private bool NotContainOnlyWhitespace(string content)
        {
            return !string.IsNullOrWhiteSpace(content);
        }
    }

    /// <summary>
    /// Validator for HistoryCreateDto - creating a search history entry
    /// </summary>
    public class HistoryCreateDtoValidator : AbstractValidator<HistoryCreateDto>
    {
        public HistoryCreateDtoValidator()
        {
            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("קטגוריה היא שדה חובה")
                .IsInEnum().WithMessage("קטגוריה לא חוקית");
        }
    }

    /// <summary>
    /// Validator for UserPreferencesDto - user preferences and statistics
    /// </summary>
    public class UserPreferencesDtoValidator : AbstractValidator<UserPreferencesDto>
    {
        public UserPreferencesDtoValidator()
        {
            // FavoriteCategory validation
            RuleFor(x => x.FavoriteCategory)
                .IsInEnum().WithMessage("קטגוריה מועדפת לא חוקית");

            // CategoryStats validation
            RuleFor(x => x.CategoryStats)
                .NotNull().WithMessage("סטטיסטיקות קטגוריות לא יכולות להיות null")
                .Must(x => x.Count <= 20).WithMessage("לא ניתן לשמור יותר מ-20 קטגוריות")
                .When(x => x.CategoryStats != null);

            // Validate each CategoryStats item
            RuleForEach(x => x.CategoryStats)
                .SetValidator(new CategoryStatsDtoValidator())
                .When(x => x.CategoryStats != null);
        }
    }

    /// <summary>
    /// Validator for CategoryStatsDto - statistics for a specific category
    /// </summary>
    public class CategoryStatsDtoValidator : AbstractValidator<CategoryStatsDto>
    {
        public CategoryStatsDtoValidator()
        {
            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("קטגוריה לא חוקית");

            RuleFor(x => x.SearchCount)
                .GreaterThanOrEqualTo(0).WithMessage("מספר חיפושים חייב להיות 0 או יותר")
                .LessThanOrEqualTo(10000).WithMessage("מספר חיפושים לא יכול להיות יותר מ-10000");
        }
    }
}