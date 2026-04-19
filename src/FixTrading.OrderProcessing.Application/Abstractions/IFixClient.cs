using FixTrading.OrderProcessing.Domain.Entities;

namespace FixTrading.OrderProcessing.Application.Abstractions;

public interface IFixClient
{
    Task<bool> SendNewOrderAsync(Order order, CancellationToken cancellationToken = default);
    bool IsConnected { get; }
}