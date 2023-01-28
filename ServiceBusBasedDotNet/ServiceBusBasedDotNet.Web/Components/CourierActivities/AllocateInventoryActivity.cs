using MassTransit;
using ServiceBusBasedDotNet.Web.Warehouse.Contracts;

namespace ServiceBusBasedDotNet.Web.Components.CourierActivities
{
    public class AllocateInventoryActivity :
        IActivity<AllocateInventoryArguments, AllocateInventoryLog>
    {
        private readonly IRequestClient<AllocateInventory> _client;
        private readonly ILogger<AllocateInventoryActivity> _logger;

        public AllocateInventoryActivity(
            IRequestClient<AllocateInventory> client,
            ILogger<AllocateInventoryActivity> logger)
        {
            _client = client; 
            _logger = logger;
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<AllocateInventoryArguments> context)
        {
            _logger.LogInformation("Executing AllocateInventoryActivity.");
            var itemNumber = context.Arguments.ItemNumber;
            var orderId = context.Arguments.OrderId;
            var quantity = context.Arguments.Quantity;
            Guid allocationId = Guid.NewGuid();
            var response = await _client.GetResponse<InventoryAllocated>(new AllocateInventory
            {
                AllocationId = allocationId,
                ItemNumber = itemNumber,
                Quantity = quantity
            });
            _logger.LogInformation("Executed AllocateInventoryActivity Get response InventoryAllocated.");
            return context.Completed(new AllocateInventoryLog
            {
                AllocationId = response.Message.AllocationId,
                OrderId = orderId,
                Quantity = quantity
            });
        }

        public async Task<CompensationResult> Compensate(CompensateContext<AllocateInventoryLog> context)
        {
            _logger.LogInformation("Compensating AllocateInventoryActivity.");
            await context.Publish(new ReleaseAllocationRequested
            {
                AllocationId = context.Log.AllocationId,
                Reason = "Compensating",
                Quantity = context.Log.Quantity,
            });

            _logger.LogInformation("Compensated AllocateInventoryActivity.");
            return context.Compensated();
        }

    }
}
