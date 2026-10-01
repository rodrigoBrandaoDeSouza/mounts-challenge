using Ambev.DeveloperEvaluation.Messaging.Events;
using Ambev.DeveloperEvaluation.Messaging.Implementations;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Rebus.Bus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Messaging;

public class MessagePublisherTests
{
    [Fact(DisplayName = "PublishAsync sends the event through the Rebus bus")]
    public async Task PublishAsync_SendsThroughBus()
    {
        var bus = Substitute.For<IBus>();
        var publisher = new MessagePublisher(bus, NullLogger<MessagePublisher>.Instance);
        var @event = new SaleCancelledEvent { SaleId = Guid.NewGuid(), SaleNumber = "S-1" };

        await publisher.PublishAsync(@event);

        await bus.Received(1).SendLocal(@event, Arg.Any<IDictionary<string, string>>());
    }

    [Fact(DisplayName = "PublishAsync rejects null messages")]
    public async Task PublishAsync_Null_Throws()
    {
        var publisher = new MessagePublisher(Substitute.For<IBus>(), NullLogger<MessagePublisher>.Instance);

        var act = () => publisher.PublishAsync<SaleCreatedEvent>(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
