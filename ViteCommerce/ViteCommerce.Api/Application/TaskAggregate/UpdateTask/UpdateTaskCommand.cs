using Database;
using MediatR;
using MongoDB.Driver;
using ViteCommerce.Api.Common.DomainAbstractions;
using ViteCommerce.Api.Common.Mappers;
using ViteCommerce.Api.Common.Models;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Application.TaskAggregate.UpdateTask;

public record class UpdateTaskCommand(
    string? Id,
    string? Text
    ) :IRequest<DomainResponse<TaskModel>>;
public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, DomainResponse<TaskModel>>
{
    private readonly IApplicationDbContext _db;

    public UpdateTaskCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }
    public async Task<DomainResponse<TaskModel>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var session = await _db.GetSessionAsync();
        var filter = Builders<TaskItem>.Filter.Eq(e => e.Id, request.Id);
        var taskItem = await _db.TaskItems.Find(session, filter).FirstOrDefaultAsync();
        if (taskItem is null)
            return default;
        request.CopyTo(taskItem);
        return taskItem.ToTaskModel();
    }
}
