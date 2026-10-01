using Ambev.DeveloperEvaluation.Messaging.Events;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.Messaging.Handlers
{
    /// <summary>
    /// Consumes the sale events from the bus and writes them to the application log.
    /// </summary>
    public class SaleEventsLogHandler :
        IHandleMessages<SaleCreatedEvent>,
        IHandleMessages<SaleModifiedEvent>,
        IHandleMessages<SaleCancelledEvent>,
        IHandleMessages<ItemCancelledEvent>,
        IHandleMessages<SaleDeletedEvent>
    {
        private readonly ILogger<SaleEventsLogHandler> _logger;

        public SaleEventsLogHandler(ILogger<SaleEventsLogHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(SaleCreatedEvent message) => Log("SaleCreated", message);
        public Task Handle(SaleModifiedEvent message) => Log("SaleModified", message);
        public Task Handle(SaleCancelledEvent message) => Log("SaleCancelled", message);
        public Task Handle(ItemCancelledEvent message) => Log("ItemCancelled", message);
        public Task Handle(SaleDeletedEvent message) => Log("SaleDeleted", message);

        private Task Log(string eventName, ISaleEvent message)
        {
            _logger.LogInformation(
                "Event {EventName} received for sale {SaleId} (occurred at {OccurredAt:o})",
                eventName,
                message.SaleId,
                message.OccurredAt);

            return Task.CompletedTask;
        }
    }
}
