using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Domain.Enums;
using FixTrading.OrderProcessing.Domain.Services;

namespace FixTrading.OrderProcessing.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ExposureCalculatorBenchmarks
{
    private Order[] _smallDataset = default!;
    private Order[] _mediumDataset = default!;
    private Order[] _largeDataset = default!;

    [GlobalSetup]
    public void Setup()
    {
        _smallDataset = GenerateOrders(100);
        _mediumDataset = GenerateOrders(1_000);
        _largeDataset = GenerateOrders(10_000);
    }

    [Benchmark(Baseline = true)]
    public decimal CalculateTotal_Small() => ExposureCalculator.CalculateTotal(_smallDataset);

    [Benchmark]
    public decimal CalculateTotal_Medium() => ExposureCalculator.CalculateTotal(_mediumDataset);

    [Benchmark]
    public decimal CalculateTotal_Large() => ExposureCalculator.CalculateTotal(_largeDataset);

    [Benchmark]
    public IReadOnlyDictionary<Symbol, decimal> CalculateBySymbol_Small() 
        => ExposureCalculator.CalculateBySymbol(_smallDataset);

    [Benchmark]
    public IReadOnlyDictionary<Symbol, decimal> CalculateBySymbol_Medium() 
        => ExposureCalculator.CalculateBySymbol(_mediumDataset);

    [Benchmark]
    public IReadOnlyDictionary<Symbol, decimal> CalculateBySymbol_Large() 
        => ExposureCalculator.CalculateBySymbol(_largeDataset);

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