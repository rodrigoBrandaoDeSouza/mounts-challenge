using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Ambev.DeveloperEvaluation.Messaging.Config
{
    /// <summary>
    /// Centralized configuration class for Rebus setup and dependency injection.
    /// </summary>
    /// 

    [ExcludeFromCodeCoverage]
    public static class MessagingConfiguration
    {
        /// <summary>
        /// Registers all messaging components, publishers, subscribers, and Rebus configuration.
        /// </summary>
        /// <param name="services">The service collection instance.</param>
        public static IServiceCollection AddMessaging(this IServiceCollection services)
        {
            // TODO: register Rebus configuration and message handlers in future commits
            return services;
        }
    }
}
