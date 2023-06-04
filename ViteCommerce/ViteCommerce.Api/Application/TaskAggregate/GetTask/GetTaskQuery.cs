using MediatR;
using ViteCommerce.Api.Common.DomainAbstractions;
using ViteCommerce.Api.Common.Models;

namespace ViteCommerce.Api.Application.ProductAggregate.GetProduct;

public record GetTaskQuery(
    string Id
    ) : IRequest<DomainResponse<TaskModel>>;
