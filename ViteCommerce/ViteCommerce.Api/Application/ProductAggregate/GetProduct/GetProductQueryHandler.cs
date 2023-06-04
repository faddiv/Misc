using Database;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using ViteCommerce.Api.Common.DomainAbstractions;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Application.ProductAggregate.GetProduct;

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, DomainResponse<Product>>
{
    private readonly IApplicationDbContext _db;

    public GetProductQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DomainResponse<Product>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var result = await _db.Products.AsQueryable()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken: cancellationToken);
        return result;
    }
}
