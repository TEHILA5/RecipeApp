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
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(2).WithMessage("Name must be at least 2 characters long")
                .MaximumLength(100).WithMessage("Name can contain up to 100 characters")
                .Matches(@"^[a-zA-Z\s]+$")
                .WithMessage("Name can contain English letters and spaces only");

            // Phone validation - Israeli format
            RuleFor(x => x.Phone).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Phone number is required")
                .Must(BeValidIsraeliPhone)
                .WithMessage("Invalid phone number. Valid formats: 05X-XXXXXXX or 0XX-XXXXXXX");

            // Email validation
            RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email address")
                .MaximumLength(100).WithMessage("Email can contain up to 100 characters");

            // Password validation - MUST be provided for registration
            RuleFor(x => x.Password).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Password is required")
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                .MaximumLength(100).WithMessage("Password can contain up to 100 characters")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase English letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase English letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit")
                .Matches(@"[!@#$%^&*(),.?""':{}|<>]")
                .WithMessage("Password must contain at least one special character");
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
            RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email address");

            RuleFor(x => x.Password).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Password is required");
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
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(2).WithMessage("Name must be at least 2 characters long")
                .MaximumLength(100).WithMessage("Name can contain up to 100 characters")
                .Matches(@"^[a-zA-Z\s]+$")
                .WithMessage("Name can contain English letters and spaces only");

            // Phone validation
            RuleFor(x => x.Phone).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Phone number is required")
                .Must(BeValidIsraeliPhone)
                .WithMessage("Invalid phone number");

            // Email validation
            RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email address")
                .MaximumLength(100).WithMessage("Email can contain up to 100 characters");

            // Password is OPTIONAL for update - only validate if provided
            When(x => !string.IsNullOrEmpty(x.Password), () =>
            {
                RuleFor(x => x.Password).Cascade(CascadeMode.Stop)
                    .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                    .MaximumLength(100).WithMessage("Password can contain up to 100 characters")
                    .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase English letter")
                    .Matches("[a-z]").WithMessage("Password must contain at least one lowercase English letter")
                    .Matches("[0-9]").WithMessage("Password must contain at least one digit")
                    .Matches(@"[!@#$%^&*(),.?""':{}|<>]")
                    .WithMessage("Password must contain at least one special character");
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