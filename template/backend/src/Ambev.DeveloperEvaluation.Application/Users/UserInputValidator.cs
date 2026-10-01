using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Users;

/// <summary>
/// Validation rules shared by the create and update user commands.
/// </summary>
public abstract class UserInputValidator<T> : AbstractValidator<T> where T : UserInput
{
    protected UserInputValidator(bool passwordRequired)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("The 'email' field must be a valid email address.")
            .MaximumLength(100);

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 50);

        if (passwordRequired)
        {
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }

        RuleFor(x => x.Password)
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(100)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.")
            .When(x => !string.IsNullOrEmpty(x.Password));

        RuleFor(x => x.Name)
            .NotNull().WithMessage("Name is required.")
            .ChildRules(name =>
            {
                name.RuleFor(n => n.Firstname).NotEmpty().WithMessage("First name is required.").MaximumLength(50);
                name.RuleFor(n => n.Lastname).NotEmpty().WithMessage("Last name is required.").MaximumLength(50);
            });

        RuleFor(x => x.Address)
            .NotNull().WithMessage("Address is required.")
            .ChildRules(address =>
            {
                address.RuleFor(a => a.City).NotEmpty().WithMessage("City is required.").MaximumLength(100);
                address.RuleFor(a => a.Street).NotEmpty().WithMessage("Street is required.").MaximumLength(100);
                address.RuleFor(a => a.Number).GreaterThan(0).WithMessage("Address number must be greater than zero.");
                address.RuleFor(a => a.Zipcode).NotEmpty().WithMessage("Zipcode is required.").MaximumLength(20);
                address.RuleFor(a => a.Geolocation).ChildRules(geo =>
                {
                    geo.RuleFor(g => g.Lat).MaximumLength(20);
                    geo.RuleFor(g => g.Long).MaximumLength(20);
                });
            });

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .Matches(@"^\+?[0-9\s\-\(\)]{8,20}$").WithMessage("The 'phone' field must be a valid phone number.");

        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status.HasValue).WithMessage("Invalid status. Use Active, Inactive or Suspended.");

        RuleFor(x => x.Role)
            .IsInEnum().When(x => x.Role.HasValue).WithMessage("Invalid role. Use Customer, Manager or Admin.");
    }
}
