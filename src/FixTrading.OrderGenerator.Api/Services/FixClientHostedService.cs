using FixTrading.OrderProcessing.Infrastructure.Fix.Client;

namespace FixTrading.OrderGenerator.Api.Services;

/// <summary>
/// Hosted Service que gerencia o lifecycle do FIX Initiator Client.
/// </summary>
public sealed class FixClientHostedService : IHostedService
{
    private readonly FixInitiatorClient _fixClient;
    private readonly ILogger<FixClientHostedService> _logger;

    public FixClientHostedService(
        FixInitiatorClient fixClient,
        ILogger<FixClientHostedService> logger)
    {
        _fixClient = fixClient;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting FIX Initiator Client...");
        
        _fixClient.Start();
        
        _logger.LogInformation("FIX Initiator Client started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping FIX Initiator Client...");
        
        _fixClient.Stop();
        
        _logger.LogInformation("FIX Initiator Client stopped");
        return Task.CompletedTask;
    }
}