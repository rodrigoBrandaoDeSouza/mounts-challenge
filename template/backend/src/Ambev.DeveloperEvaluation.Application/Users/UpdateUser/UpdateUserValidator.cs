using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Users.UpdateUser;

public class UpdateUserValidator : UserInputValidator<UpdateUserCommand>
{
    public UpdateUserValidator() : base(passwordRequired: false)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("User ID must be greater than zero.");
    }
}
