using FluentValidation;
using RecipeApp.Common.DTOs;
using System.Text.RegularExpressions;

namespace RecipeApp.Service.Validators
{
    // <summary>
    /// Validator for User DTO - handles registration and profile updates
    /// </summary>
    public class UserDtoValidator : AbstractValidator<UserCreateDto>
    {
        public UserDtoValidator()
        {
            // Name validation
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("שם הוא שדה חובה")
                .MinimumLength(2).WithMessage("שם חייב להכיל לפחות 2 תווים")
                .MaximumLength(100).WithMessage("שם יכול להכיל עד 100 תווים")
                .Matches(@"^[\u0590-\u05FFa-zA-Z\s]+$").WithMessage("שם יכול להכיל רק אותיות ורווחים");

            // Phone validation - Israeli format
            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("מספר טלפון הוא שדה חובה")
                .Must(BeValidIsraeliPhone).WithMessage("מספר טלפון לא תקין. פורמט נכון: 05X-XXXXXXX או 0XX-XXXXXXX");

            // Email validation
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("אימייל הוא שדה חובה")
                .EmailAddress().WithMessage("כתובת אימייל לא תקינה")
                .MaximumLength(100).WithMessage("אימייל יכול להכיל עד 100 תווים");

            // Password validation - MUST be provided for registration
            RuleFor(x => x.Password)
                .NotNull().WithMessage("סיסמה היא שדה חובה")
                .NotEmpty().WithMessage("סיסמה היא שדה חובה")
                .MinimumLength(8).WithMessage("סיסמה חייבת להכיל לפחות 8 תווים")
                .MaximumLength(100).WithMessage("סיסמה יכולה להכיל עד 100 תווים")
                .Matches("[A-Z]").WithMessage("סיסמה חייבת להכיל לפחות אות גדולה אחת באנגלית")
                .Matches("[a-z]").WithMessage("סיסמה חייבת להכיל לפחות אות קטנה אחת באנגלית")
                .Matches("[0-9]").WithMessage("סיסמה חייבת להכיל לפחות ספרה אחת")
                .Matches(@"[!@#$%^&*(),.?""':{}|<>]").WithMessage("סיסמה חייבת להכיל לפחות תו מיוחד אחד");
        }

        /// <summary>
        /// Validates Israeli phone numbers (mobile and landline)
        /// Accepts formats: 0501234567, 050-1234567, 02-1234567
        /// </summary>
        private bool BeValidIsraeliPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Remove spaces and dashes
            phone = phone.Replace("-", "").Replace(" ", "").Trim();

            // Check length (9-10 digits)
            if (phone.Length < 9 || phone.Length > 10)
                return false;

            // Must start with 0
            if (!phone.StartsWith("0"))
                return false;

            // All characters must be digits
            if (!phone.All(char.IsDigit))
                return false;

            // Mobile: 050-059 (10 digits)
            if (phone.StartsWith("05") && phone.Length == 10)
                return true;

            // Landline: 02/03/04/08/09 (9 digits)
            if ((phone.StartsWith("02") || phone.StartsWith("03") ||
                 phone.StartsWith("04") || phone.StartsWith("08") ||
                 phone.StartsWith("09")) && phone.Length == 9)
                return true;

            return false;
        }
    }

    /// <summary>
    /// Validator for login requests
    /// </summary>
    public class LoginDtoValidator : AbstractValidator<UserLoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("אימייל הוא שדה חובה")
                .EmailAddress().WithMessage("כתובת אימייל לא תקינה");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("סיסמה היא שדה חובה");
        }
    }

    /// <summary>
    /// Validator for User Profile Update - password is optional
    /// </summary>
    public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
    {
        public UserUpdateDtoValidator()
        {
            // Name validation
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("שם הוא שדה חובה")
                .MinimumLength(2).WithMessage("שם חייב להכיל לפחות 2 תווים")
                .MaximumLength(100).WithMessage("שם יכול להכיל עד 100 תווים");

            // Phone validation
            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("מספר טלפון הוא שדה חובה")
                .Must(BeValidIsraeliPhone).WithMessage("מספר טלפון לא תקין");

            // Email validation
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("אימייל הוא שדה חובה")
                .EmailAddress().WithMessage("כתובת אימייל לא תקינה")
                .MaximumLength(100).WithMessage("אימייל יכול להכיל עד 100 תווים");

            // Password is OPTIONAL for update - only validate if provided
            When(x => !string.IsNullOrEmpty(x.Password), () =>
            {
                RuleFor(x => x.Password)
                    .MinimumLength(8).WithMessage("סיסמה חייבת להכיל לפחות 8 תווים")
                    .MaximumLength(100).WithMessage("סיסמה יכולה להכיל עד 100 תווים")
                    .Matches("[A-Z]").WithMessage("סיסמה חייבת להכיל לפחות אות גדולה אחת")
                    .Matches("[a-z]").WithMessage("סיסמה חייבת להכיל לפחות אות קטנה אחת")
                    .Matches("[0-9]").WithMessage("סיסמה חייבת להכיל לפחות ספרה אחת")
                    .Matches(@"[!@#$%^&*(),.?""':{}|<>]").WithMessage("סיסמה חייבת להכיל תו מיוחד");
            });
        }

        private bool BeValidIsraeliPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Remove spaces and dashes
            phone = phone.Replace("-", "").Replace(" ", "").Trim();

            // Check length (9-10 digits)
            if (phone.Length < 9 || phone.Length > 10)
                return false;

            // Must start with 0
            if (!phone.StartsWith("0"))
                return false;

            // All characters must be digits
            if (!phone.All(char.IsDigit))
                return false;

            // Mobile: 050-059 (10 digits)
            if (phone.StartsWith("05") && phone.Length == 10)
                return true;

            // Landline: 02/03/04/08/09 (9 digits)
            if ((phone.StartsWith("02") || phone.StartsWith("03") ||
                 phone.StartsWith("04") || phone.StartsWith("08") ||
                 phone.StartsWith("09")) && phone.Length == 9)
                return true;

            return false;
        }
    }
} 