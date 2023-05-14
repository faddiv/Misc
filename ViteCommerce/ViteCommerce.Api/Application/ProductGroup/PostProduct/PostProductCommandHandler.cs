using Database;
using Mediator;
using ViteCommerce.Api.Common.Mappers;
using ViteCommerce.Api.Common.ValidationResults;

namespace ViteCommerce.Api.Application.ProductGroup.PostProduct;

public class PostProductCommandHandler : ICommandHandler<PostProductCommand, IDomainResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _unitOfWork;

    public PostProductCommandHandler(IApplicationDbContext db, IUnitOfWork unitOfWork)
    {
        _db = db;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<IDomainResponse> Handle(
        PostProductCommand command, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.GetSessionAsync();
        var product = command.ToProduct();
        await _db.Products.InsertOneAsync(session, product, cancellationToken: cancellationToken);
        return DomainResponses.Wrap(product);
    }
}
