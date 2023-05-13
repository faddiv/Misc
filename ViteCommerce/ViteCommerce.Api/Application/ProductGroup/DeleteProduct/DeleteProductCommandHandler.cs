using Catalog.API.Entities;
using Database;
using Mediator;
using MongoDB.Driver;
using ViteCommerce.Api.Common.ValidationResults;

namespace ViteCommerce.Api.Application.ProductGroup.DeleteProduct;

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, NotFoundOr<Unit>>
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IApplicationDbContext db, IUnitOfWork unitOfWork)
    {
        _db = db;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<NotFoundOr<Unit>> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.GetSessionAsync();
        var builder = new FilterDefinitionBuilder<Product>().Eq(e => e.Id, command.Id);
        var result = await _db.Products.DeleteOneAsync(session, builder);
        
        return result.DeletedCount == 1 ? Unit.Value : NotFoundOr<Unit>.NotFoundResponse();
    }
}
