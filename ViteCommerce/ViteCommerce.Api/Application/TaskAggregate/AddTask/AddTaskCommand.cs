using MediatR;
using ViteCommerce.Api.Common.DomainAbstractions;
using ViteCommerce.Api.Common.Models;

namespace ViteCommerce.Api.Application.ProductAggregate.PostProduct;

public record AddTaskCommand(
    string? Text
    ) : IRequest<DomainResponse<TaskModel>>;
