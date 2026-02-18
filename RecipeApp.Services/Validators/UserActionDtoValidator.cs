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
                .IsInEnum().WithMessage("Invalid action type");

            // RecipeId validation - optional field
            RuleFor(x => x.RecipeId)
                .GreaterThan(0).WithMessage("Recipe ID must be greater than 0")
                .When(x => x.RecipeId.HasValue);

            // RecipeName validation - optional field
            RuleFor(x => x.RecipeName)
                .MaximumLength(200).WithMessage("Recipe name can contain up to 200 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.RecipeName));

            // RecipeImageUrl validation - optional field
            RuleFor(x => x.RecipeImageUrl)
                .MaximumLength(500).WithMessage("Image URL can contain up to 500 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.RecipeImageUrl));

            // Category validation - optional field
            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Invalid category")
                .When(x => x.Category.HasValue);

            // UserName validation - optional field
            RuleFor(x => x.UserName).Cascade(CascadeMode.Stop)
                .MinimumLength(2).WithMessage("User name must be at least 2 characters long")
                .MaximumLength(100).WithMessage("User name can contain up to 100 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.UserName));

            // Content validation for comments - optional field
            RuleFor(x => x.Content).Cascade(CascadeMode.Stop)
                .MinimumLength(2).WithMessage("Content must be at least 2 characters long")
                .MaximumLength(2000).WithMessage("Content can contain up to 2000 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Content));

            // Rating validation - optional field
            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5")
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
            RuleFor(x => x.RecipeId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Recipe ID is required")
                .GreaterThan(0).WithMessage("Recipe ID must be greater than 0");
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
            RuleFor(x => x.RecipeId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Recipe ID is required")
                .GreaterThan(0).WithMessage("Recipe ID must be greater than 0");

            // Content is required
            RuleFor(x => x.Content).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Comment content is required")
                .MinimumLength(2).WithMessage("Comment must be at least 2 characters long")
                .MaximumLength(2000).WithMessage("Comment can contain up to 2000 characters")
                .Must(NotContainOnlyWhitespace)
                .WithMessage("Comment cannot contain only whitespace");

            // Rating is required
            RuleFor(x => x.Rating).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Rating is required")
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5 stars");
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
            RuleFor(x => x.Category).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Category is required")
                .IsInEnum().WithMessage("Invalid category");
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
                .IsInEnum().WithMessage("Invalid favorite category");

            // CategoryStats validation
            RuleFor(x => x.CategoryStats).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Category statistics cannot be null")
                .Must(x => x.Count <= 20).WithMessage("Cannot store more than 20 categories")
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
                .IsInEnum().WithMessage("Invalid category");

            RuleFor(x => x.SearchCount).Cascade(CascadeMode.Stop)
                .GreaterThanOrEqualTo(0).WithMessage("Search count must be 0 or greater")
                .LessThanOrEqualTo(10000).WithMessage("Search count cannot exceed 10000");
        }
    }
}