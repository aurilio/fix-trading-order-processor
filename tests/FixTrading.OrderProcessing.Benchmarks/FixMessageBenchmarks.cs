using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Infrastructure.Fix.Messages;
using QuickFix.Fields;
using FixMessage = QuickFix.FIX44;
using DomainSymbol = FixTrading.OrderProcessing.Domain.Enums.Symbol;
using DomainOrderSide = FixTrading.OrderProcessing.Domain.Enums.OrderSide;

namespace FixTrading.OrderProcessing.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class FixMessageBenchmarks
{
    private Order _order = default!;
    private FixMessage.NewOrderSingle _fixMessage = default!;

    [GlobalSetup]
    public void Setup()
    {
        _order = Order.Create(DomainSymbol.PETR4, DomainOrderSide.Buy, 100, 35.50m);
        _fixMessage = FixMessageBuilder.BuildNewOrderSingle(_order);
    }

    [Benchmark(Baseline = true)]
    public FixMessage.NewOrderSingle BuildNewOrderSingle()
    {
        return FixMessageBuilder.BuildNewOrderSingle(_order);
    }

    [Benchmark]
    public FixMessage.ExecutionReport BuildExecutionReport()
    {
        return FixMessageBuilder.BuildExecutionReport(
            _order,
            Guid.NewGuid().ToString("N")[..12],
            ExecType.FILL,
            OrdStatus.FILLED);
    }

    [Benchmark]
    public Order ParseNewOrderSingle()
    {
        return FixMessageParser.ParseNewOrderSingle(_fixMessage);
    }

    [Benchmark]
    [Arguments(100)]
    [Arguments(1_000)]
    public FixMessage.NewOrderSingle[] BuildMultipleMessages(int count)
    {
        var messages = new FixMessage.NewOrderSingle[count];
        for (int i = 0; i < count; i++)
        {
            messages[i] = FixMessageBuilder.BuildNewOrderSingle(_order);
        }
        return messages;
    }
}