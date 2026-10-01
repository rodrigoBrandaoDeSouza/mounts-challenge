using Ambev.DeveloperEvaluation.Messaging.Config;
using Microsoft.AspNetCore.Builder;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

public class MessagingModuleInitializer : IModuleInitializer
{
    public void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddMessaging();
    }
}
