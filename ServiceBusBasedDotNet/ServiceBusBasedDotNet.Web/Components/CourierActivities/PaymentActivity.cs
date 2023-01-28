using MassTransit;

namespace ServiceBusBasedDotNet.Web.Components.CourierActivities;

public class PaymentActivity : IActivity<PaymentArguments, PaymentLog>
{
    private readonly ILogger<PaymentActivity> _logger;

    public PaymentActivity(ILogger<PaymentActivity> logger)
    {
        _logger = logger;
    }
    public async Task<ExecutionResult> Execute(ExecuteContext<PaymentArguments> context)
    {
        string cardNumber = context.Arguments.CardNumber;

        if(string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 5)
        {
            _logger.LogInformation($"Fail payment with Invalid card for {context.Arguments.CardNumber}");
            throw new ApplicationException("Invalid card");
        }

        await Task.Delay(100);

        _logger.LogInformation($"Payment completed for {context.Arguments.CardNumber}");
        return context.Completed(new PaymentLog
        {
            AuthorizationCode = "OK"
        });
    }

    public async Task<CompensationResult> Compensate(CompensateContext<PaymentLog> context)
    {
        _logger.LogInformation($"Compensate Payment for {context.Log.AuthorizationCode}");
        await Task.Delay(100);

        return context.Compensated();
    }

}
