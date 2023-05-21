using Database;
using MediatR;
using ViteCommerce.Api.Common.Mappers;
using ViteCommerce.Api.Common.ValidationResults;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Application.ProductGroup.PostProduct;

public class PostProductCommandHandler : IRequestHandler<PostProductCommand, DomainResponseBase<Product>>
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _unitOfWork;

    public PostProductCommandHandler(IApplicationDbContext db, IUnitOfWork unitOfWork)
    {
        _db = db;
        _unitOfWork = unitOfWork;
    }

    public async Task<DomainResponseBase<Product>> Handle(PostProductCommand request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.GetSessionAsync();
        var product = request.ToProduct();
        await _db.Products.InsertOneAsync(session, product, cancellationToken: cancellationToken);
        return DomainResponses.Wrap(product);
    }
}
