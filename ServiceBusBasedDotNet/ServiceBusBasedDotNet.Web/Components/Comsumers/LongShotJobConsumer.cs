using MassTransit;
using ServiceBusBasedDotNet.Web.MessageContracts;

namespace ServiceBusBasedDotNet.Web.Components.Comsumers;

public class LongShotJobConsumer : IJobConsumer<LongShotJob>
{
    private readonly ILogger<LongShotJobConsumer> _logger;

    public LongShotJobConsumer(ILogger<LongShotJobConsumer> logger)
    {
        _logger = logger;
    }
    public async Task Run(JobContext<LongShotJob> context)
    {
        for (int i = 0; i < context.Job.Seconds; i++)
        {
            _logger.LogInformation("Tick {Second} on Retry {Retry}", i, context.RetryAttempt);
            await Task.Delay(1000);
            if(context.Job.ShouldFail && context.RetryAttempt == 0)
            {
                throw new ApplicationException("ApplicationException Occured");
            }
        }
        _logger.LogInformation("Long job finised");
    }
}
public class LongShotJobConsumerDefinition : ConsumerDefinition<LongShotJobConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<LongShotJobConsumer> consumerConfigurator)
    {
        consumerConfigurator.Options<JobOptions<LongShotJob>>(options =>
        {
            options.SetRetry(r => r.Interval(2, 1000))
            .SetJobTimeout(TimeSpan.FromMinutes(3))
            .SetConcurrentJobLimit(10);
        });
    }
}
