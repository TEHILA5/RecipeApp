using FluentValidation;
using Microsoft.AspNetCore.Http;
using RecipeApp.Common.DTOs;

namespace RecipeApp.Service.Validators
{
    /// <summary>
    /// Validator ליצירת מתכון עם תמונה
    /// </summary>
    public class RecipeCreateFormDtoValidator : AbstractValidator<RecipeCreateFormDto>
    {
        public RecipeCreateFormDtoValidator()
        {
            // Name validation
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("שם המתכון הוא שדה חובה")
                .MinimumLength(2).WithMessage("שם המתכון חייב להכיל לפחות 2 תווים")
                .MaximumLength(200).WithMessage("שם המתכון יכול להכיל עד 200 תווים");

            // Description validation
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("תיאור יכול להכיל עד 1000 תווים")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            // Category validation
            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("קטגוריה לא חוקית");

            // Instructions validation
            RuleFor(x => x.Instructions)
                .NotEmpty().WithMessage("הוראות הכנה הן שדה חובה")
                .MinimumLength(10).WithMessage("הוראות הכנה חייבות להכיל לפחות 10 תווים");

            // Image validation - either ImageFile OR ImageUrl must be provided
            RuleFor(x => x)
                .Must(x => x.ImageFile != null || !string.IsNullOrWhiteSpace(x.ImageUrl))
                .WithMessage("חייב לספק תמונה - קובץ או קישור");

            // ImageFile validation (if provided)
            RuleFor(x => x.ImageFile)
                .Must(BeValidImageFile).WithMessage("קובץ לא תקין. רק JPG, PNG, GIF, WEBP מותרים")
                .Must(BeValidImageSize).WithMessage("גודל התמונה חייב להיות קטן מ-5MB")
                .When(x => x.ImageFile != null);

            // ImageUrl validation (if provided)
            RuleFor(x => x.ImageUrl)
                .MaximumLength(500).WithMessage("כתובת תמונה יכולה להכיל עד 500 תווים")
                .Must(BeValidUrl).WithMessage("כתובת URL לא תקינה")
                .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

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
                .LessThanOrEqualTo(1440).WithMessage("זמן הכנה לא יכול להיות יותר מ-24 שעות");

            // TotalTime validation
            RuleFor(x => x.TotalTime)
                .GreaterThan(0).WithMessage("זמן כולל חייב להיות גדול מ-0")
                .LessThanOrEqualTo(2880).WithMessage("זמן כולל לא יכול להיות יותר מ-48 שעות");

            // TotalTime >= PrepTime
            RuleFor(x => x.TotalTime)
                .GreaterThanOrEqualTo(x => x.PrepTime)
                .WithMessage("זמן כולל חייב להיות גדול או שווה לזמן ההכנה");
        }

        private bool BeValidImageFile(IFormFile? file)
        {
            if (file == null) return true;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return allowedExtensions.Contains(extension);
        }

        private bool BeValidImageSize(IFormFile? file)
        {
            if (file == null) return true;

            const int maxSizeInBytes = 5 * 1024 * 1024; // 5MB
            return file.Length <= maxSizeInBytes;
        }

        private bool BeValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }

    /// <summary>
    /// Validator לעדכון מתכון עם תמונה
    /// </summary>
    public class RecipeUpdateFormDtoValidator : AbstractValidator<RecipeUpdateFormDto>
    {
        public RecipeUpdateFormDtoValidator()
        {
            // Name validation (optional)
            RuleFor(x => x.Name)
                .MinimumLength(2).WithMessage("שם המתכון חייב להכיל לפחות 2 תווים")
                .MaximumLength(200).WithMessage("שם המתכון יכול להכיל עד 200 תווים")
                .When(x => !string.IsNullOrWhiteSpace(x.Name));

            // Description validation (optional)
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("תיאור יכול להכיל עד 1000 תווים")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            // Category validation (optional)
            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("קטגוריה לא חוקית")
                .When(x => x.Category.HasValue);

            // Instructions validation (optional)
            RuleFor(x => x.Instructions)
                .MinimumLength(10).WithMessage("הוראות הכנה חייבות להכיל לפחות 10 תווים")
                .When(x => !string.IsNullOrWhiteSpace(x.Instructions));

            // ImageFile validation (if provided)
            RuleFor(x => x.ImageFile)
                .Must(BeValidImageFile).WithMessage("קובץ לא תקין. רק JPG, PNG, GIF, WEBP מותרים")
                .Must(BeValidImageSize).WithMessage("גודל התמונה חייב להיות קטן מ-5MB")
                .When(x => x.ImageFile != null);

            // ImageUrl validation (if provided)
            RuleFor(x => x.ImageUrl)
                .MaximumLength(500).WithMessage("כתובת תמונה יכולה להכיל עד 500 תווים")
                .Must(BeValidUrl).WithMessage("כתובת URL לא תקינה")
                .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

            // Servings validation (optional)
            RuleFor(x => x.Servings)
                .GreaterThan(0).WithMessage("מספר מנות חייב להיות גדול מ-0")
                .LessThanOrEqualTo(100).WithMessage("מספר מנות לא יכול להיות יותר מ-100")
                .When(x => x.Servings.HasValue);

            // Level validation (optional)
            RuleFor(x => x.Level)
                .InclusiveBetween(1, 5).WithMessage("רמת קושי חייבת להיות בין 1 ל-5")
                .When(x => x.Level.HasValue);

            // PrepTime validation (optional)
            RuleFor(x => x.PrepTime)
                .GreaterThan(0).WithMessage("זמן הכנה חייב להיות גדול מ-0")
                .LessThanOrEqualTo(1440).WithMessage("זמן הכנה לא יכול להיות יותר מ-24 שעות")
                .When(x => x.PrepTime.HasValue);

            // TotalTime validation (optional)
            RuleFor(x => x.TotalTime)
                .GreaterThan(0).WithMessage("זמן כולל חייב להיות גדול מ-0")
                .LessThanOrEqualTo(2880).WithMessage("זמן כולל לא יכול להיות יותר מ-48 שעות")
                .When(x => x.TotalTime.HasValue);

            // TotalTime >= PrepTime (if both provided)
            RuleFor(x => x.TotalTime)
                .GreaterThanOrEqualTo(x => x.PrepTime)
                .WithMessage("זמן כולל חייב להיות גדול או שווה לזמן ההכנה")
                .When(x => x.TotalTime.HasValue && x.PrepTime.HasValue);
        }

        private bool BeValidImageFile(IFormFile? file)
        {
            if (file == null) return true;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return allowedExtensions.Contains(extension);
        }

        private bool BeValidImageSize(IFormFile? file)
        {
            if (file == null) return true;

            const int maxSizeInBytes = 5 * 1024 * 1024; // 5MB
            return file.Length <= maxSizeInBytes;
        }

        private bool BeValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}