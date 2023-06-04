using Database;
using MediatR;
using MongoDB.Driver;
using ViteCommerce.Api.Common.DomainAbstractions;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Application.ProductAggregate.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteTaskCommand, DomainResponse<bool>>
{
    private readonly IApplicationDbContext _db;

    public DeleteProductCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DomainResponse<bool>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var session = await _db.GetSessionAsync(cancellationToken);
        var builder = new FilterDefinitionBuilder<TaskItem>().Eq(e => e.Id, request.Id);
        var result = await _db.TaskItems.DeleteOneAsync(session, builder);

        return result.DeletedCount == 1
            ? DomainResponses.OkOrEmpty(true)
            : DomainResponses.NotFound<bool>();
    }
}
