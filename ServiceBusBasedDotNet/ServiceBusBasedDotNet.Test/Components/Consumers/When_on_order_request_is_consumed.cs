using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceBusBasedDotNet.Web.MessageContracts;

namespace ServiceBusBasedDotNet.Web.Components.Consumers;

public class When_on_order_request_is_consumed
{
    [Fact]
    public async Task Should_consume_the_message()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddLogging();
                cfg.AddConsumer<SubmitOrderConsumer>();
            })
            .BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        var consumer = harness.GetConsumerHarness<SubmitOrderConsumer>();

        await harness.Start();
        try
        {
            await harness.Bus.Publish(new SubmitOrder
            {
                OrderId = Guid.NewGuid(),
                CustomerNumber = "1234",
                Timestamp = DateTime.Now
            });

            consumer.Consumed.Select<SubmitOrder>().Should().HaveCount(1);
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task Should_publish_the_message()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddLogging();
                cfg.AddConsumer<SubmitOrderConsumer>();
            })
            .BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        var consumer = harness.GetConsumerEndpoint<SubmitOrderConsumer>();

        await harness.Start();
        try
        {
            await harness.Bus.Publish(new SubmitOrder
            {
                OrderId = Guid.NewGuid(),
                CustomerNumber = "1234",
                Timestamp = DateTime.Now
            });

            harness.Published.Select<OrderSubmitted>().Should().HaveCount(1);
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task Should_publish_the_rejected_message()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddLogging();
                cfg.AddConsumer<SubmitOrderConsumer>();
            })
            .BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        var consumer = harness.GetConsumerEndpoint<SubmitOrderConsumer>();

        await harness.Start();
        try
        {
            await harness.Bus.Publish(new SubmitOrder
            {
                OrderId = Guid.NewGuid(),
                CustomerNumber = "asdf",
                Timestamp = DateTime.Now
            });

            harness.Published.Select<OrderRejected>().Should().HaveCount(1);
        }
        finally
        {
            await harness.Stop();
        }
    }
    [Fact]
    public async Task Should_respond_with_accepted()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddLogging();
                cfg.AddConsumer<SubmitOrderBasicConsumer>();
            })
            .BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        var consumer = harness.GetConsumerEndpoint<SubmitOrderBasicConsumer>();

        await harness.Start();
        try
        {
            var requestClient = harness.GetRequestClient<SubmitOrderBasic>();
            var message = new SubmitOrderBasic
            {
                OrderId = Guid.NewGuid(),
                CustomerNumber = "1234",
                Timestamp = DateTime.Now
            };
            var response = await requestClient.GetResponse<OrderAccepted>(message);

            response.Message.Should().NotBeNull();
            response.Message.OrderId.Should().Be(message.OrderId);
            response.Message.CustomerNumber.Should().Be(message.CustomerNumber);
            response.Message.Timestamp.Should().Be(message.Timestamp);
        }
        finally
        {
            await harness.Stop();
        }
    }


    [Fact]
    public async Task Should_respond_with_rejected()
    {
        var loggerFactory = LoggerFactory.Create(e => { });
        var harness = new InMemoryTestHarness();
        var consumer = harness.Consumer(() =>
            new SubmitOrderBasicConsumer(loggerFactory.CreateLogger<SubmitOrderBasicConsumer>()));

        await harness.Start();
        try
        {
            var requestClient = await harness.ConnectRequestClient<SubmitOrderBasic>();
            var message = new SubmitOrderBasic
            {
                OrderId = Guid.NewGuid(),
                CustomerNumber = "asdf",
                Timestamp = DateTime.Now
            };
            var response = await requestClient.GetResponse<OrderRejected>(message);

            response.Message.Should().NotBeNull();
            response.Message.OrderId.Should().Be(message.OrderId);
            response.Message.Reason.Should().Be("Banned: asdf");
            response.Message.Timestamp.Should().Be(message.Timestamp);
        }
        finally
        {
            await harness.Stop();
        }
    }
}
