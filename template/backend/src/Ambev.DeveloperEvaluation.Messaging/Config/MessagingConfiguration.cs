using Ambev.DeveloperEvaluation.Messaging.Handlers;
using Ambev.DeveloperEvaluation.Messaging.Implementations;
using Ambev.DeveloperEvaluation.Messaging.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Transport.InMem;
using System.Diagnostics.CodeAnalysis;

namespace Ambev.DeveloperEvaluation.Messaging.Config
{
    /// <summary>
    /// Rebus setup and dependency injection of the messaging components.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class MessagingConfiguration
    {
        public const string InputQueueName = "developer-evaluation-events";

        /// <summary>
        /// Registers Rebus (in-memory transport), the event handlers and the <see cref="IMessagePublisher"/>.
        /// </summary>
        public static IServiceCollection AddMessaging(this IServiceCollection services)
        {
            services.AddRebus(configure => configure
                .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), InputQueueName)));

            services.AutoRegisterHandlersFromAssemblyOf<SaleEventsLogHandler>();
            services.AddScoped<IMessagePublisher, MessagePublisher>();

            return services;
        }
    }
}
