using Database;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using ViteCommerce.Api.Common.DomainAbstractions;
using ViteCommerce.Api.Common.Mappers;
using ViteCommerce.Api.Common.Models;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Application.ProductAggregate.GetProduct;

public class GetTaskQueryHandler : IRequestHandler<GetTaskQuery, DomainResponse<TaskModel>>
{
    private readonly IApplicationDbContext _db;

    public GetTaskQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DomainResponse<TaskModel>> Handle(GetTaskQuery request, CancellationToken cancellationToken)
    { 
        var result = await _db.TaskItems.AsQueryable()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken: cancellationToken);
        return result.ToTaskModel();
    }
}
