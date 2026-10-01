using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Products;

/// <summary>
/// Validation rules shared by the create and update product commands.
/// </summary>
public abstract class ProductInputValidator<T> : AbstractValidator<T> where T : ProductInput
{
    protected ProductInputValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("The 'price' field must be a positive number.");

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(100);

        RuleFor(x => x.Image)
            .MaximumLength(500)
            .Must(BeAValidUrl).When(x => !string.IsNullOrWhiteSpace(x.Image))
            .WithMessage("The 'image' field must be a valid absolute URL.");

        RuleFor(x => x.Rating)
            .NotNull().WithMessage("Rating is required.");

        RuleFor(x => x.Rating.Rate)
            .InclusiveBetween(0m, 5m).When(x => x.Rating is not null)
            .WithMessage("The 'rating.rate' field must be between 0 and 5.");

        RuleFor(x => x.Rating.Count)
            .GreaterThanOrEqualTo(0).When(x => x.Rating is not null)
            .WithMessage("The 'rating.count' field must not be negative.");
    }

    private static bool BeAValidUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
