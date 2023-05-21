using Database;
using Mediator;
using ViteCommerce.Api.Common.Mappers;
using ViteCommerce.Api.Common.ValidationResults;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Application.ProductGroup.PostProduct;

public class PostProductCommandHandler :
    ICommandHandler<PostProductCommand, SelfContainedDomainResponse<Product>>
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _unitOfWork;

    public PostProductCommandHandler(IApplicationDbContext db, IUnitOfWork unitOfWork)
    {
        _db = db;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<SelfContainedDomainResponse<Product>> Handle(
        PostProductCommand command, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.GetSessionAsync();
        var product = command.ToProduct();
        await _db.Products.InsertOneAsync(session, product, cancellationToken: cancellationToken);
        return DomainResponses.Wrap(product);
    }
}
