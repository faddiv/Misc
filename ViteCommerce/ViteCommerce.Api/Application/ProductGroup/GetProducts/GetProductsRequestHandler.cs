using Database;
using MediatR;
using MongoDB.Driver;
using ViteCommerce.Api.Common.DomainAbstractions;

namespace ViteCommerce.Api.Application.ProductGroup.GetProducts;

public class GetProductsRequestHandler : IRequestHandler<GetProductsRequest, DomainResponse<GetProductsResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _unitOfWork;

    public GetProductsRequestHandler(IApplicationDbContext db, IUnitOfWork unitOfWork)
    {
        _db = db;
        _unitOfWork = unitOfWork;
    }

    public async Task<DomainResponse<GetProductsResponse>> Handle(GetProductsRequest request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.GetSessionAsync(cancellationToken);
        var result = await _db.Products.AsQueryable(session)
            .ToListAsync(cancellationToken: cancellationToken);
        return new GetProductsResponse
        {
            Data = result
        };
    }
}
