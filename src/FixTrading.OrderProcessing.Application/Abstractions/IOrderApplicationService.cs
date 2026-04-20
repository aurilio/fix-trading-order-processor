using FixTrading.OrderProcessing.Application.Contracts;

namespace FixTrading.OrderProcessing.Application.Abstractions;

public interface IOrderApplicationService
{
    Task<SendOrderResponse> SendOrderAsync(SendOrderRequest request,  CancellationToken cancellationToken = default);
}