using FluentValidation;

namespace ViteCommerce.Api.Application.ProductGroup.PostProduct;

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
