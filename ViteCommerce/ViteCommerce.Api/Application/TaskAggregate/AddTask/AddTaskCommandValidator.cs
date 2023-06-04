using FluentValidation;

namespace ViteCommerce.Api.Application.ProductAggregate.PostProduct;

public class AddTaskCommandValidator : AbstractValidator<AddTaskCommand>
{
    public AddTaskCommandValidator()
    {
        RuleFor(e => e.Text)
            .NotEmpty();
    }
}
