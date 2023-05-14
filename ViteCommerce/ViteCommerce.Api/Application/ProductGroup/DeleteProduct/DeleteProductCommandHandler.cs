using Database;
using Mediator;
using MongoDB.Driver;
using ViteCommerce.Api.Common.ValidationResults;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Application.ProductGroup.DeleteProduct;

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, IDomainResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IApplicationDbContext db, IUnitOfWork unitOfWork)
    {
        _db = db;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<IDomainResponse> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.GetSessionAsync();
        var builder = new FilterDefinitionBuilder<Product>().Eq(e => e.Id, command.Id);
        var result = await _db.Products.DeleteOneAsync(session, builder);
        
        return result.DeletedCount == 1
            ? DomainResponses.Ok
            : DomainResponses.NotFound;
    }
}
