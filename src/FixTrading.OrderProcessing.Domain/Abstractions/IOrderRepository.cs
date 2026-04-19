using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Domain.Enums;

namespace FixTrading.OrderProcessing.Domain.Abstractions;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    Task<Order?> GetByClOrdIdAsync(string clOrdId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Order>> GetBySymbolAsync(Symbol symbol, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Order>> GetAllAsync(CancellationToken cancellationToken = default);
}