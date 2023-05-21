using Database;
using Mediator;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using ViteCommerce.Api.Common.ValidationResults;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Application.ProductGroup.GetProduct;

public class GetProductQueryHandler : IQueryHandler<GetProductQuery, SelfContainedDomainResponse<Product>>
{
    private readonly IApplicationDbContext _db;

    public GetProductQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<SelfContainedDomainResponse<Product>> Handle
        (GetProductQuery query, CancellationToken cancellationToken)
    {
        var result = await _db.Products.AsQueryable().FirstOrDefaultAsync(e => e.Id == query.Id, cancellationToken: cancellationToken);
        return DomainResponses.Wrap(result);
    }
}
