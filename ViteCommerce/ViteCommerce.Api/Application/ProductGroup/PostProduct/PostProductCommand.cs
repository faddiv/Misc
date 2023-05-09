using Mediator;

namespace ViteCommerce.Api.Application.ProductGroup.PostProduct;


public class PostProductCommand : ICommand<PostProductResponse>
{
    public string Id { get; set; } = "";

    public string Name { get; set; } = "";

    public string Category { get; set; } = "";

    public string? Description { get; set; } = "";

    public string? ImageFile { get; set; } = "";

    public decimal Price { get; set; }
}


/*public record PostProductCommand(
    string? Name,
    string? Category,
    string? Description,
    string? ImageFile,
    decimal? Price
    ) : ICommand<PostProductResponse>;
*/
public class PostProductResponse
{
    public string Id { get; set; }
}

public class PostProductCommandHandler : ICommandHandler<PostProductCommand, PostProductResponse>
{
    public ValueTask<PostProductResponse> Handle(PostProductCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
