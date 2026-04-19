using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Domain.Enums;

namespace FixTrading.OrderProcessing.Domain.Services;

public static class ExposureCalculator
{
    // Consolida o saldo financeiro (net exposure) agrupado por ativo
    public static IReadOnlyDictionary<Symbol, decimal> CalculateBySymbol(
        IEnumerable<Order> orders)
    {
        ArgumentNullException.ThrowIfNull(orders);

        return orders
            .GroupBy(o => o.Symbol)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(o => o.CalculateExposure()));
    }

    public static decimal CalculateTotal(IEnumerable<Order> orders)
    {
        ArgumentNullException.ThrowIfNull(orders);
        return orders.Sum(o => o.CalculateExposure());
    }
}