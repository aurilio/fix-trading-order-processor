using Bogus;
using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Domain.Enums;
using FixTrading.OrderProcessing.Domain.ValueObjects;

namespace FixTrading.OrderProcessing.Domain.Tests.Fixtures;

public sealed class OrderFaker : Faker<Order>
{
    private static readonly Symbol[] ValidSymbols = [Symbol.PETR4, Symbol.VALE3, Symbol.VIIA4];

    public OrderFaker()
    {
        CustomInstantiator(f =>
        {
            var symbol = f.PickRandom(ValidSymbols);
            var side = f.PickRandom<OrderSide>();
            var quantity = f.Random.Int(Quantity.MinValue, Quantity.MaxValue);
            var price = Math.Round(f.Random.Decimal(Price.MinValue, Price.MaxValue), 2);

            return Order.Create(symbol, side, quantity, price);
        });
    }

    public Order WithSide(OrderSide side) =>
        Order.Create(
            FakerHub.PickRandom(ValidSymbols),
            side,
            FakerHub.Random.Int(Quantity.MinValue, 1000),
            Math.Round(FakerHub.Random.Decimal(Price.MinValue, 100m), 2));

    public Order WithSymbol(Symbol symbol) =>
        Order.Create(
            symbol,
            FakerHub.PickRandom<OrderSide>(),
            FakerHub.Random.Int(Quantity.MinValue, 1000),
            Math.Round(FakerHub.Random.Decimal(Price.MinValue, 100m), 2));

    public Order WithValues(Symbol symbol, OrderSide side, int quantity, decimal price) =>
        Order.Create(symbol, side, quantity, price);
}