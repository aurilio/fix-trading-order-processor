using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Domain.Enums;
using FixTrading.OrderProcessing.Infrastructure.Fix.Repositories;

namespace FixTrading.OrderProcessing.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class RepositoryBenchmarks
{
    private InMemoryOrderRepository _repository = default!;
    private Order[] _orders = default!;
    private Order _singleOrder = default!;

    [Params(100, 1_000, 10_000)]
    public int OrderCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _repository = new InMemoryOrderRepository();
        _orders = GenerateOrders(OrderCount);
        _singleOrder = Order.Create(Symbol.PETR4, OrderSide.Buy, 100, 35.50m);

        foreach (var order in _orders)
        {
            _repository.AddAsync(order).GetAwaiter().GetResult();
        }
    }

    [Benchmark(Baseline = true)]
    public async Task AddOrder()
    {
        var order = Order.Create(Symbol.PETR4, OrderSide.Buy, 100, 35.50m);
        await _repository.AddAsync(order);
    }

    [Benchmark]
    public async Task<IReadOnlyCollection<Order>> GetAllOrders()
    {
        return await _repository.GetAllAsync();
    }

    [Benchmark]
    public async Task<IReadOnlyCollection<Order>> GetBySymbol()
    {
        return await _repository.GetBySymbolAsync(Symbol.PETR4);
    }

    [Benchmark]
    public async Task<Order?> GetByClOrdId()
    {
        return await _repository.GetByClOrdIdAsync(_orders[0].ClOrdId);
    }

    [Benchmark]
    public async Task ConcurrentAddAndRead()
    {
        var addTask = Task.Run(async () =>
        {
            for (int i = 0; i < 10; i++)
            {
                var order = Order.Create(Symbol.VALE3, OrderSide.Sell, i + 1, 10.00m);
                await _repository.AddAsync(order);
            }
        });

        var readTask = Task.Run(async () =>
        {
            for (int i = 0; i < 10; i++)
            {
                await _repository.GetAllAsync();
            }
        });

        await Task.WhenAll(addTask, readTask);
    }

    private static Order[] GenerateOrders(int count)
    {
        var symbols = new[] { Symbol.PETR4, Symbol.VALE3, Symbol.VIIA4 };
        var sides = new[] { OrderSide.Buy, OrderSide.Sell };

        return Enumerable.Range(0, count)
            .Select(i => Order.Create(
                symbols[i % symbols.Length],
                sides[i % sides.Length],
                (i % 1000) + 1,
                Math.Round((i % 100) + 0.01m, 2)))
            .ToArray();
    }
}