using FluentAssertions;
using MassTransit;
using MassTransit.SagaStateMachine;
using MassTransit.Testing;
using MassTransit.Visualizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceBusBasedDotNet.Web.Components.Consumers;
using ServiceBusBasedDotNet.Web.MessageContracts;

namespace ServiceBusBasedDotNet.Web.Components.StateMachines;

public class Submitting_an_order
{
    [Fact]
    public async Task Should_create_a_state_instance()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddLogging();
                cfg.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        var saga = harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        var stateMachine = saga.StateMachine;

        await harness.Start();
        try
        {
            var correlationId = Guid.NewGuid();
            await harness.Bus.Publish(new SubmitOrder
            {
                OrderId = correlationId,
                CustomerNumber = "1234",
                Timestamp = DateTime.Now
            });

            await saga.Exists(correlationId, stateMachine.Received);
            var instance = saga.Created.Contains(correlationId);
            instance.Should().NotBeNull();
            instance.CurrentState.Should().Be(stateMachine.Received.Name);
        }
        finally
        {
            await harness.Stop();
        }
    }


    [Fact]
    public async Task Should_responds_to_status_checks()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddLogging();
                cfg.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        var saga = harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        var stateMachine = saga.StateMachine;

        await harness.Start();
        try
        {
            var correlationId = Guid.NewGuid();
            await harness.Bus.Publish(new SubmitOrder
            {
                OrderId = correlationId,
                CustomerNumber = "1234",
                Timestamp = DateTime.Now
            });

            await saga.Exists(correlationId, stateMachine.Received);

            var client = harness.GetRequestClient<CheckOrder>();
            var response = await client.GetResponse<OrderStatus>(new CheckOrder
            {
                OrderId = correlationId
            });
            response.Should().NotBeNull();
            response.Message.State.Should().Be(stateMachine.Received.Name);
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task Should_be_submitted_when_order_submitted()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddLogging();
                cfg.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        var saga = harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        var stateMachine = saga.StateMachine;

        await harness.Start();
        try
        {
            var correlationId = Guid.NewGuid();
            await harness.Bus.Publish(new SubmitOrder
            {
                OrderId = correlationId,
                CustomerNumber = "1234",
                Timestamp = DateTime.Now
            });

            await saga.Exists(correlationId, stateMachine.Received);
            await harness.Bus.Publish(new OrderSubmitted
            {
                OrderId = correlationId,
                CustomerNumber = "1234",
                Timestamp = DateTime.Now
            });
            await saga.Exists(correlationId, stateMachine.Submitted);

            var client = harness.GetRequestClient<CheckOrder>();
            var response = await client.GetResponse<OrderStatus>(new CheckOrder
            {
                OrderId = correlationId
            });
            response.Should().NotBeNull();
            response.Message.State.Should().Be(stateMachine.Submitted.Name);
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task Should_be_cancel_when_account_closed()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddLogging();
                cfg.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        var sagaHarness = harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        var stateMachine = sagaHarness.StateMachine;

        await harness.Start();
        try
        {
            var correlationId = Guid.NewGuid();

            await harness.Bus.Publish(new SubmitOrder
            {
                OrderId = correlationId,
                CustomerNumber = "1234",
                Timestamp = DateTime.Now
            });

            await sagaHarness.Exists(correlationId, stateMachine.Received);
            await harness.Bus.Publish(new OrderSubmitted
            {
                OrderId = correlationId,
                CustomerNumber = "1234",
                Timestamp = DateTime.Now
            });
            await sagaHarness.Exists(correlationId, stateMachine.Submitted);
            await harness.Bus.Publish(new CustomerAccountClosed
            {
                CustomerId = Guid.NewGuid(),
                CustomerNumber = "1234"
            });
            await sagaHarness.Exists(correlationId, stateMachine.Cancelled);

            var client = harness.GetRequestClient<CheckOrder>();
            var response = await client.GetResponse<OrderStatus>(new CheckOrder
            {
                OrderId = correlationId
            });
            response.Should().NotBeNull();
            response.Message.State.Should().Be(stateMachine.Cancelled.Name);
        }
        finally
        {
            await harness.Stop();
        }
    }
    [Fact]
    public async Task WriteGraph()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddLogging();
                cfg.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        var sagaHarness = harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        var stateMachine = sagaHarness.StateMachine;

        var graph = stateMachine.GetGraph();
        var gen = new StateMachineGraphvizGenerator(graph);
        File.WriteAllText("orderStateMachine.dot", gen.CreateDotFile());
    }
}
