using FixTrading.OrderProcessing.Application.Contracts;
using FixTrading.OrderProcessing.Domain.Abstractions;
using FixTrading.OrderProcessing.Domain.Services;
using Microsoft.Extensions.Logging;

namespace FixTrading.OrderProcessing.Application.Services;

public sealed class ExposureApplicationService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<ExposureApplicationService> _logger;

    public ExposureApplicationService(
        IOrderRepository orderRepository,
        ILogger<ExposureApplicationService> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<ExposureResponse> GetExposureAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);

        var exposureBySymbol = ExposureCalculator.CalculateBySymbol(orders);
        var totalExposure = ExposureCalculator.CalculateTotal(orders);

        _logger.LogDebug("Calculated exposure for {Count} orders. Total: {Total:C}", 
            orders.Count, totalExposure);

        var exposureDict = exposureBySymbol.ToDictionary(
            kvp => kvp.Key.ToString(),
            kvp => kvp.Value);

        return new ExposureResponse(exposureDict, totalExposure);
    }
}