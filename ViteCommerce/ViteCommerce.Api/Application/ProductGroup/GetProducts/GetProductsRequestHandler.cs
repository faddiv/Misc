using Database;
using Mediator;
using MongoDB.Driver;
using ViteCommerce.Api.Application.ProductGroup;

namespace ViteCommerce.Api.Application.ProductGroup.GetProducts;

public class GetProductsRequestHandler : IQueryHandler<GetProductsRequest, GetProductsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _unitOfWork;

    public GetProductsRequestHandler(IApplicationDbContext db, IUnitOfWork unitOfWork)
    {
        _db = db;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<GetProductsResponse> Handle(GetProductsRequest request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.GetSessionAsync();
        var result = await _db.Products.AsQueryable(session)
            .ToListAsync();
        return new GetProductsResponse
        {
            Data = result
        };
    }
}
