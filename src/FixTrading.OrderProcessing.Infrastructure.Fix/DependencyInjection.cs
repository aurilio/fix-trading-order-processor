using FixTrading.OrderProcessing.Application.Abstractions;
using FixTrading.OrderProcessing.Domain.Abstractions;
using FixTrading.OrderProcessing.Infrastructure.Fix.Client;
using FixTrading.OrderProcessing.Infrastructure.Fix.Configuration;
using FixTrading.OrderProcessing.Infrastructure.Fix.Repositories;
using FixTrading.OrderProcessing.Infrastructure.Fix.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FixTrading.OrderProcessing.Infrastructure.Fix;

public static class DependencyInjection
{
    public static IServiceCollection AddFixInitiator(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddFixCore(configuration);
        services.AddSingleton<FixInitiatorClient>();
        services.AddSingleton<IFixClient>(sp => sp.GetRequiredService<FixInitiatorClient>());

        return services;
    }

    public static IServiceCollection AddFixAcceptor(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddFixCore(configuration);
        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
        services.AddSingleton<FixAcceptorServer>();

        return services;
    }

    private static IServiceCollection AddFixCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FixSettings>(configuration.GetSection(FixSettings.SectionName));
        services.TryAddSingleton<TimeProvider>(TimeProvider.System);

        return services;
    }
}