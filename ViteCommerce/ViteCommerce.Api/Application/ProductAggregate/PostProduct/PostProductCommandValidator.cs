using FluentValidation;

namespace ViteCommerce.Api.Application.ProductAggregate.PostProduct;

public class PostProductCommandValidator : AbstractValidator<PostProductCommand>
{
    public PostProductCommandValidator()
    {
        RuleFor(e => e.Name)
            .NotEmpty();

        RuleFor(e => e.Category)
            .NotEmpty();

        RuleFor(e => e.Price)
            .NotEmpty()
            .GreaterThan(0);
    }
}
