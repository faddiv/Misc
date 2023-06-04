using Riok.Mapperly.Abstractions;
using ViteCommerce.Api.Application.ProductAggregate.PostProduct;
using ViteCommerce.Api.Application.TaskAggregate.UpdateTask;
using ViteCommerce.Api.Common.Models;
using ViteCommerce.Api.Entities;

namespace ViteCommerce.Api.Common.Mappers;

[Mapper]
public static partial class TaskMapper
{
    public static partial TaskModel? ToTaskModel(this TaskItem? taskItem);

    public static partial TaskItem ToTaskItem(this AddTaskCommand command);

    [MapperIgnoreTarget(nameof(TaskItem.Id))]
    public static partial void CopyTo(this UpdateTaskCommand command, TaskItem taskItem);
}
