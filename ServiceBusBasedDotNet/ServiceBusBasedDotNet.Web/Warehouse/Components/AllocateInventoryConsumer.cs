using MassTransit;
using ServiceBusBasedDotNet.Web.Warehouse.Contracts;

namespace ServiceBusBasedDotNet.Web.Warehouse.Components;

public class AllocateInventoryConsumer : IConsumer<AllocateInventory>
{
    private readonly ILogger<AllocateInventoryConsumer> _logger;

    public AllocateInventoryConsumer(ILogger<AllocateInventoryConsumer> logger)
    {
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<AllocateInventory> context)
    {
        _logger.LogInformation("Consuming AllocateInventoryConsumer.");
        await Task.Delay(100);

        await context.Publish(new AllocationCreated
        {
            AllocationId = context.Message.AllocationId,
            HoldDuration = TimeSpan.FromMilliseconds(8000),
            Quantity = context.Message.Quantity
        });
        await context.RespondAsync(new InventoryAllocated
        {
            AllocationId = context.Message.AllocationId,
            ItemNumber = context.Message.ItemNumber,
            Quantity = context.Message.Quantity
        });
        _logger.LogInformation("Consumed AllocateInventoryConsumer responded with InventoryAllocated.");
    }
}
