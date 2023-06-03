using Database;
using MediatR;
using MongoDB.Driver;
using ViteCommerce.Api.Common.DomainAbstractions;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Application.ProductGroup.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, DomainResponse<bool>>
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IApplicationDbContext db, IUnitOfWork unitOfWork)
    {
        _db = db;
        _unitOfWork = unitOfWork;
    }

    public async Task<DomainResponse<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.GetSessionAsync(cancellationToken);
        var builder = new FilterDefinitionBuilder<Product>().Eq(e => e.Id, request.Id);
        var result = await _db.Products.DeleteOneAsync(session, builder);

        return result.DeletedCount == 1
            ? DomainResponses.OkOrEmpty(true)
            : DomainResponses.NotFound<bool>();
    }
}
