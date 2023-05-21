using Database;
using MediatR;
using MongoDB.Driver;
using ViteCommerce.Api.Common.ValidationResults;

namespace ViteCommerce.Api.Application.ProductGroup.GetProducts;

public class GetProductsRequestHandler : IRequestHandler<GetProductsRequest, DomainResponseBase<GetProductsResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _unitOfWork;

    public GetProductsRequestHandler(IApplicationDbContext db, IUnitOfWork unitOfWork)
    {
        _db = db;
        _unitOfWork = unitOfWork;
    }

    public async Task<DomainResponseBase<GetProductsResponse>> Handle(GetProductsRequest request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.GetSessionAsync();
        var result = await _db.Products.AsQueryable(session)
            .ToListAsync(cancellationToken: cancellationToken);
        return DomainResponses.Wrap(new GetProductsResponse
        {
            Data = result
        });
    }
}
