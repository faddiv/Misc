using MassTransit;
using MassTransit.Courier.Contracts;
using MassTransit.Internals;
using MassTransit.MessageData;
using MassTransit.MessageData.Configuration;
using ServiceBusBasedDotNet.Web;
using ServiceBusBasedDotNet.Web.Components.BatchConsumers;
using ServiceBusBasedDotNet.Web.MessageContracts;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddMassTransit(cfg =>
{
    cfg.SetKebabCaseEndpointNameFormatter();

    // By default, sagas are in-memory, but should be changed to a durable
    // saga repository.
    cfg.SetInMemorySagaRepositoryProvider();
    

    var entryAssembly = typeof(WebApis).Assembly;

    cfg.AddMediator();
    cfg.AddConsumers(entryAssembly);
    cfg.AddSagaStateMachines(entryAssembly);
    cfg.AddSagas(entryAssembly);
    cfg.AddActivities(entryAssembly);
    var messageData = new InMemoryMessageDataRepository();
    cfg.AddSingleton<IMessageDataRepository>(messageData);
    //cfg.AddRequestClient<SubmitOrderBasic>(); //Not needed.
    MessageDataDefaults.AlwaysWriteToRepository = true;
    MessageDataDefaults.ExtraTimeToLive = TimeSpan.FromMinutes(1);


    cfg.UsingRabbitMq((context, rcfg) =>
    {
        var configuration = context.GetRequiredService<IConfiguration>();
        rcfg.Host(configuration.GetConnectionString("rabbitmq"));
        rcfg.UseDelayedMessageScheduler();
        rcfg.ConfigureEndpoints(context);
        rcfg.UseMessageData(selector =>
        {
            return messageData;
        });
    });
    /*cfg.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });*/
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var etcGroup = app.MapGroup("/etc")
    //.WithGroupName("Etc")
    .WithTags("Etc")
    .WithOpenApi();

etcGroup.MapGet("/weatherforecast", WebApis.GetWeather);
etcGroup.MapPost("/post-order", WebApis.PostOrder);
etcGroup.MapPost("/send-order", WebApis.SendOrder);
etcGroup.MapPost("/publish-order", WebApis.PublishOrder);

var orderGroup = app.MapGroup("/order")
    //.WithGroupName("Order")
    .WithTags("Order")
    .WithOpenApi();

orderGroup.MapPost("/submit-order", WebApis.SubmitOrder);

orderGroup.MapGet("/get-order-state/{correlationId}", WebApis.GetOrderState);

orderGroup.MapDelete("/order/close-customer-account/{customerNumber}", WebApis.DeleteCustomer);

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Run app failed");
}
