using Database;
using Mediator;
using ViteCommerce.Api.Common;
using ViteCommerce.Api.Common.Mappers;

namespace ViteCommerce.Api.Application.ProductGroup.PostProduct;

public class PostProductCommandHandler : ICommandHandler<PostProductCommand, InvalidOr<PostProductResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _unitOfWork;

    public PostProductCommandHandler(IApplicationDbContext db, IUnitOfWork unitOfWork)
    {
        _db = db;
        _unitOfWork = unitOfWork;
    }
    public async ValueTask<InvalidOr<PostProductResponse>> Handle(
        PostProductCommand command, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.GetSessionAsync();
        var product = command.ToProduct();
        await _db.Products.InsertOneAsync(session, product);
        return new PostProductResponse { Id = product.Id };
    }
}
