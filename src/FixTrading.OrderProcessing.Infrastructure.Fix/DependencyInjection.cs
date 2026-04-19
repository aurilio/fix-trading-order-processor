using FixTrading.OrderProcessing.Application.Abstractions;
using FixTrading.OrderProcessing.Domain.Abstractions;
using FixTrading.OrderProcessing.Infrastructure.Fix.Client;
using FixTrading.OrderProcessing.Infrastructure.Fix.Configuration;
using FixTrading.OrderProcessing.Infrastructure.Fix.Repositories;
using FixTrading.OrderProcessing.Infrastructure.Fix.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FixTrading.OrderProcessing.Infrastructure.Fix;

public static class DependencyInjection
{
    public static IServiceCollection AddFixInitiator(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FixSettings>(configuration.GetSection(FixSettings.SectionName));
        services.AddSingleton<FixInitiatorClient>();
        services.AddSingleton<IFixClient>(sp => sp.GetRequiredService<FixInitiatorClient>());

        return services;
    }

    public static IServiceCollection AddFixAcceptor(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FixSettings>(configuration.GetSection(FixSettings.SectionName));
        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<FixAcceptorServer>();

        return services;
    }
}