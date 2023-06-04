using Database;
using MediatR;
using MongoDB.Driver;
using ViteCommerce.Api.Common.DomainAbstractions;
using ViteCommerce.Api.Common.Mappers;
using ViteCommerce.Api.Common.Models;

namespace ViteCommerce.Api.Application.TaskAggregate.GetTasks;

public record class GetTasksRequest : IRequest<DomainResponse<List<TaskModel>>>;
public class GetTasksRequestHandler : IRequestHandler<GetTasksRequest, DomainResponse<List<TaskModel>>>
{
    private readonly IApplicationDbContext _db;

    public GetTasksRequestHandler(IApplicationDbContext db)
    {
        _db = db;
    }
    public async Task<DomainResponse<List<TaskModel>>> Handle(GetTasksRequest request, CancellationToken cancellationToken)
    {
        var session = await _db.GetSessionAsync(cancellationToken);
        var tasks = await _db.TaskItems.AsQueryable(session).ToListAsync(cancellationToken);
        return tasks.Select(e => e.ToTaskModel())
            .ToList();
    }
}
