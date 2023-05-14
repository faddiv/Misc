using Database;
using Mediator;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using ViteCommerce.Api.Common.ValidationResults;

namespace ViteCommerce.Api.Application.ProductGroup.GetProduct;

public class GetProductQueryHandler : IQueryHandler<GetProductQuery, IDomainResponse>
{
    private readonly IApplicationDbContext _db;

    public GetProductQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<IDomainResponse> Handle(GetProductQuery query, CancellationToken cancellationToken)
    {
        var result = await _db.Products.AsQueryable().FirstOrDefaultAsync(e => e.Id == query.Id, cancellationToken: cancellationToken);
        return DomainResponses.Wrap(result);
    }
}
