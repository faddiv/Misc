using Database;
using MediatR;
using ViteCommerce.Api.Common.Mappers;
using ViteCommerce.Api.Common.DomainAbstractions;
using ViteCommerce.Api.Entities;
using ViteCommerce.Api.Common.Models;

namespace ViteCommerce.Api.Application.ProductAggregate.PostProduct;

public class AddTaskCommandHandler : IRequestHandler<AddTaskCommand, DomainResponse<TaskModel>>
{
    private readonly IApplicationDbContext _db;

    public AddTaskCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DomainResponse<TaskModel>> Handle(AddTaskCommand request, CancellationToken cancellationToken)
    {
        var session = await _db.GetSessionAsync(cancellationToken);
        var taskItem = request.ToTaskItem();
        await _db.TaskItems.InsertOneAsync(session, taskItem, cancellationToken: cancellationToken);
        return taskItem.ToTaskModel();
    }
}
