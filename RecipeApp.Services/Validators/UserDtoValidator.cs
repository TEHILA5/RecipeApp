using FluentValidation;
using RecipeApp.Common.DTOs;
using RecipeApp.Service.Validators.Helpers;

namespace RecipeApp.Service.Validators
{
    public class UserDtoValidator : AbstractValidator<UserCreateDto>
    {
        public UserDtoValidator()
        {
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(2).WithMessage("Name must be at least 2 characters long")
                .MaximumLength(100).WithMessage("Name can contain up to 100 characters")
                .Matches(@"^[a-zA-Z\s]+$")
                .WithMessage("Name can contain English letters and spaces only");

            RuleFor(x => x.Phone).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Phone number is required")
                .Must(PhoneValidator.IsValid)
                .WithMessage("Invalid phone number. Valid formats: Israeli (05X-XXXXXXX, 0XX-XXXXXXX) or US ((XXX) XXX-XXXX)");

            RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email address")
                .MaximumLength(100).WithMessage("Email can contain up to 100 characters");

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
    }

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

    public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
    {
        public UserUpdateDtoValidator()
        {
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(2).WithMessage("Name must be at least 2 characters long")
                .MaximumLength(100).WithMessage("Name can contain up to 100 characters")
                .Matches(@"^[a-zA-Z\s]+$")
                .WithMessage("Name can contain English letters and spaces only");

            RuleFor(x => x.Phone).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Phone number is required")
                .Must(PhoneValidator.IsValid)
                .WithMessage("Invalid phone number. Valid formats: Israeli (05X-XXXXXXX, 0XX-XXXXXXX) or US ((XXX) XXX-XXXX)");

            RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email address")
                .MaximumLength(100).WithMessage("Email can contain up to 100 characters");

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
    }
}