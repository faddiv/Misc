using Database;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using ViteCommerce.Api.Common.ValidationResults;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Application.ProductGroup.GetProduct;

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, DomainResponseBase<Product>>
{
    private readonly IApplicationDbContext _db;

    public GetProductQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DomainResponseBase<Product>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var result = await _db.Products.AsQueryable().FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken: cancellationToken);
        return DomainResponses.Wrap(result);
    }
}
