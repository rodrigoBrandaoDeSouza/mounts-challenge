using Ambev.DeveloperEvaluation.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Messaging.Implementations
{
    public class MessagePublisher : IMessagePublisher
    {
        private readonly ILogger<MessagePublisher> _logger;

        public MessagePublisher(ILogger<MessagePublisher> logger)
        {
            _logger = logger;
        }

        public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Publish here on the true message broker");
            return Task.CompletedTask;
        }
    }
}
