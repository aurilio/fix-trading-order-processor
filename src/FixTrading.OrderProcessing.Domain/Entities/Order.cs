using FixTrading.OrderProcessing.Domain.Enums;
using FixTrading.OrderProcessing.Domain.ValueObjects;

namespace FixTrading.OrderProcessing.Domain.Entities;

public sealed class Order
{
    public string ClOrdId { get; }
    public Symbol Symbol { get; }
    public OrderSide Side { get; }
    public Quantity Quantity { get; }
    public Price Price { get; }
    public DateTime CreatedAtUtc { get; }

    private Order(
        string clOrdId,
        Symbol symbol,
        OrderSide side,
        Quantity quantity,
        Price price,
        DateTime createdAtUtc)
    {
        ClOrdId = clOrdId;
        Symbol = symbol;
        Side = side;
        Quantity = quantity;
        Price = price;
        CreatedAtUtc = createdAtUtc;
    }

    public static Order Create(
        Symbol symbol,
        OrderSide side,
        int quantity,
        decimal price,
        TimeProvider? timeProvider = null)
    {
        var clock = timeProvider ?? TimeProvider.System;
        var now = clock.GetUtcNow().UtcDateTime;
        
        var clOrdId = GenerateClOrdId(now);
        var qty = Quantity.Create(quantity);
        var prc = Price.Create(price);

        return new Order(clOrdId, symbol, side, qty, prc, now);
    }

    public decimal CalculateFinancialValue() => Price.Value * Quantity.Value;

    public decimal CalculateExposure() => Side == OrderSide.Buy
        ? CalculateFinancialValue()
        : -CalculateFinancialValue();

    private static string GenerateClOrdId(DateTime timestamp) =>
        $"ORD-{timestamp:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}