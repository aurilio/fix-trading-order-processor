using FixTrading.Ordergenerator.Api;
using FixTrading.OrderProcessing.Application.Abstractions;
using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Infrastructure.Fix.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace FixTrading.OrderGenerator.Api.Integration.Tests.Fixtures;

public class OrderGeneratorApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private IFixClient _mockFixClient = default!;

    public IFixClient MockFixClient => _mockFixClient;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Remove o FixInitiatorClient real
            services.RemoveAll<FixInitiatorClient>();
            services.RemoveAll<IFixClient>();

            // Remove o HostedService que inicia o FIX Client real
            services.RemoveAll<IHostedService>();

            // Cria mock do IFixClient
            _mockFixClient = Substitute.For<IFixClient>();
            _mockFixClient.IsConnected.Returns(true);
            _mockFixClient.SendNewOrderAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
                .Returns(true);

            // Registra o mock
            services.AddSingleton(_mockFixClient);

            // Adiciona TimeProvider
            services.RemoveAll<TimeProvider>();
            services.AddSingleton(TimeProvider.System);
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public new Task DisposeAsync() => Task.CompletedTask;

    public void SetupFixClientFailure()
    {
        _mockFixClient.SendNewOrderAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
            .Returns(false);
    }

    public void SetupFixClientSuccess()
    {
        _mockFixClient.SendNewOrderAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
            .Returns(true);
    }

    public void ResetFixClient()
    {
        _mockFixClient.ClearReceivedCalls();
        SetupFixClientSuccess();
    }
}