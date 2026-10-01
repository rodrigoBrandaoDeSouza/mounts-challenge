namespace Ambev.DeveloperEvaluation.Application.Users.CreateUser;

public class CreateUserValidator : UserInputValidator<CreateUserCommand>
{
    public CreateUserValidator() : base(passwordRequired: true)
    {
    }
}
