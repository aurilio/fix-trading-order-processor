using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Domain.Enums;
using FixTrading.OrderProcessing.Domain.ValueObjects;

namespace FixTrading.OrderProcessing.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class OrderCreationBenchmarks
{
    private static readonly Symbol[] Symbols = [Symbol.PETR4, Symbol.VALE3, Symbol.VIIA4];
    private static readonly OrderSide[] Sides = [OrderSide.Buy, OrderSide.Sell];

    [Benchmark(Baseline = true)]
    public Order CreateSingleOrder()
    {
        return Order.Create(Symbol.PETR4, OrderSide.Buy, 100, 35.50m);
    }

    [Benchmark]
    [Arguments(100)]
    [Arguments(1_000)]
    [Arguments(10_000)]
    public Order[] CreateMultipleOrders(int count)
    {
        var orders = new Order[count];
        for (int i = 0; i < count; i++)
        {
            orders[i] = Order.Create(
                Symbols[i % Symbols.Length],
                Sides[i % Sides.Length],
                (i % 1000) + 1,
                Math.Round((i % 100) + 0.01m, 2));
        }
        return orders;
    }

    [Benchmark]
    public Price CreatePrice() => Price.Create(35.50m);

    [Benchmark]
    public Quantity CreateQuantity() => Quantity.Create(100);

    [Benchmark]
    public (Price, Quantity) CreateValueObjects()
    {
        var price = Price.Create(35.50m);
        var quantity = Quantity.Create(100);
        return (price, quantity);
    }
}