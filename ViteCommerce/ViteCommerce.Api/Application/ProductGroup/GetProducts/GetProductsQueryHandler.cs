using Database;
using Mediator;
using MongoDB.Driver;
using ViteCommerce.Api.Common.ValidationResults;

namespace ViteCommerce.Api.Application.ProductGroup.GetProducts;

public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, IDomainResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _unitOfWork;

    public GetProductsQueryHandler(IApplicationDbContext db, IUnitOfWork unitOfWork)
    {
        _db = db;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<IDomainResponse> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.GetSessionAsync();
        var result = await _db.Products.AsQueryable(session)
            .ToListAsync(cancellationToken: cancellationToken);
        return new GetProductsResponse
        {
            Data = result
        };
    }
}
