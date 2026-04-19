using System.Collections.Concurrent;
using FixTrading.OrderProcessing.Domain.Abstractions;
using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Domain.Enums;

namespace FixTrading.OrderProcessing.Infrastructure.Fix.Repositories;


// Repositório de ordens em memória.
// Thread-safe para uso em cenários concorrentes.
public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<string, Order> _orders = new();

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _orders.TryAdd(order.ClOrdId, order);
        return Task.CompletedTask;
    }

    public Task<Order?> GetByClOrdIdAsync(string clOrdId, CancellationToken cancellationToken = default)
    {
        _orders.TryGetValue(clOrdId, out var order);
        return Task.FromResult(order);
    }

    public Task<IReadOnlyCollection<Order>> GetBySymbolAsync(Symbol symbol, CancellationToken cancellationToken = default)
    {
        var orders = _orders.Values
            .Where(o => o.Symbol == symbol)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<Order>>(orders);
    }

    public Task<IReadOnlyCollection<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = _orders.Values.ToArray();
        return Task.FromResult<IReadOnlyCollection<Order>>(orders);
    }
}