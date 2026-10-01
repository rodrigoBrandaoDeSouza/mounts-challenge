using Ambev.DeveloperEvaluation.Messaging.Interfaces;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.Messaging.Implementations
{
    /// <summary>
    /// Publishes the events through Rebus. The bus uses the in-memory transport, so no message
    /// broker is required: the events are delivered to the local handlers, which write them to the
    /// application log. Replacing the transport (RabbitMQ, Azure Service Bus...) is a configuration change.
    /// </summary>
    public class MessagePublisher : IMessagePublisher
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly IBus _bus;
        private readonly ILogger<MessagePublisher> _logger;

        public MessagePublisher(IBus bus, ILogger<MessagePublisher> logger)
        {
            _bus = bus;
            _logger = logger;
        }

        public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class
        {
            ArgumentNullException.ThrowIfNull(message);

            _logger.LogInformation(
                "Publishing event {EventName}: {EventPayload}",
                typeof(TMessage).Name,
                JsonSerializer.Serialize(message, JsonOptions));

            await _bus.SendLocal(message);
        }
    }
}
