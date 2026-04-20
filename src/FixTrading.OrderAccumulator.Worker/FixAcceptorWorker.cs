using FixTrading.OrderProcessing.Application.Abstractions;
using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Infrastructure.Fix.Server;

namespace FixTrading.OrderAccumulator.Worker;

// Worker Service que executa o FIX Acceptor e processa ordens recebidas.
public sealed class FixAcceptorWorker : BackgroundService
{
    private readonly FixAcceptorServer _acceptorServer;
    private readonly IExposureApplicationService _exposureService;
    private readonly ILogger<FixAcceptorWorker> _logger;
    
    private int _orderCount;
    private decimal _lastTotalExposure;

    public FixAcceptorWorker(
        FixAcceptorServer acceptorServer,
        IExposureApplicationService exposureService,
        ILogger<FixAcceptorWorker> logger)
    {
        _acceptorServer = acceptorServer;
        _exposureService = exposureService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FIX Acceptor Worker starting...");

        // Subscription no ExecuteAsync
        _acceptorServer.OnOrderReceived += OnOrderReceivedAsync;

        try
        {
            _acceptorServer.Start();
            _logger.LogInformation("FIX Acceptor Server started. Listening for orders...");

            // Mantém o worker ativo
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("FIX Acceptor Worker received shutdown signal");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FIX Acceptor Worker");
            throw;
        }
        finally
        {
            // Clean da subscription
            _acceptorServer.OnOrderReceived -= OnOrderReceivedAsync;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("FIX Acceptor Worker stopping...");
        
        _acceptorServer.Stop();
        
        await LogFinalExposureSummaryAsync(cancellationToken);
        
        await base.StopAsync(cancellationToken);
        
        _logger.LogInformation("FIX Acceptor Worker stopped. Total orders processed: {OrderCount}", _orderCount);
    }

    // Handler chamado quando uma ordem é recebida via FIX.
    private async Task OnOrderReceivedAsync(Order order, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _orderCount);

        _logger.LogInformation(
            "Order #{OrderNumber} received - ClOrdId: {ClOrdId}, {Symbol} {Side} {Qty}@{Price:F2} = {Value:C}",
            _orderCount,
            order.ClOrdId,
            order.Symbol,
            order.Side,
            order.Quantity.Value,
            order.Price.Value,
            order.CalculateFinancialValue());

        await LogExposureChangeAsync(cancellationToken);
    }


    // Loga exposição apenas quando há mudança significativa.
    private async Task LogExposureChangeAsync(CancellationToken cancellationToken)
    {
        try
        {
            var exposure = await _exposureService.GetExposureAsync(cancellationToken);

            if (exposure.TotalExposure != _lastTotalExposure)
            {
                _lastTotalExposure = exposure.TotalExposure;

                _logger.LogInformation(
                    "Exposure Updated - Total: {TotalExposure:C} | By Symbol: {ExposureBySymbol}",
                    exposure.TotalExposure,
                    FormatExposureBySymbol(exposure.ExposureBySymbol));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to calculate exposure");
        }
    }

    private async Task LogFinalExposureSummaryAsync(CancellationToken cancellationToken)
    {
        try
        {
            var exposure = await _exposureService.GetExposureAsync(cancellationToken);

            _logger.LogInformation(
                "Final Exposure Summary - Orders: {OrderCount} | Total Exposure: {TotalExposure:C}",
                _orderCount,
                exposure.TotalExposure);

            foreach (var (symbol, value) in exposure.ExposureBySymbol)
            {
                _logger.LogInformation("  {Symbol}: {Exposure:C}", symbol, value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log final exposure summary");
        }
    }

    private static string FormatExposureBySymbol(IReadOnlyDictionary<string, decimal> exposureBySymbol)
    {
        return string.Join(" | ", exposureBySymbol.Select(kvp => $"{kvp.Key}: {kvp.Value:C}"));
    }

    public override void Dispose()
    {
        _acceptorServer.Dispose();
        base.Dispose();
    }
}