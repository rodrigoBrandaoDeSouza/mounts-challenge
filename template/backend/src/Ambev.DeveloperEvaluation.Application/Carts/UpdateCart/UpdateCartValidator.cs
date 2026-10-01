using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;

public class UpdateCartValidator : CartInputValidator<UpdateCartCommand>
{
    public UpdateCartValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Cart ID must be greater than zero.");
    }
}
