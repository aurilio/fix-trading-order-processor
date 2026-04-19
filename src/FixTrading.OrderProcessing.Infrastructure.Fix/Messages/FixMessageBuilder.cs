using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Domain.Enums;
using QuickFix.Fields;
using FixMessage = QuickFix.FIX44;

namespace FixTrading.OrderProcessing.Infrastructure.Fix.Messages;

// Criar mensagens FIX a partir de entidades do domínio.
public static class FixMessageBuilder
{
    public static FixMessage.NewOrderSingle BuildNewOrderSingle(Order order)
    {
        var message = new FixMessage.NewOrderSingle(
            new ClOrdID(order.ClOrdId),
            new QuickFix.Fields.Symbol(order.Symbol.ToString()),
            new Side(order.Side.ToFixValue()),
            new TransactTime(order.CreatedAtUtc),
            new OrdType(OrdType.LIMIT))
        {
            OrderQty = new OrderQty(order.Quantity.Value),
            Price = new Price(order.Price.Value),
            TimeInForce = new TimeInForce(TimeInForce.DAY),
            HandlInst = new HandlInst(HandlInst.AUTOMATED_EXECUTION_ORDER_PRIVATE)
        };

        return message;
    }

    public static FixMessage.ExecutionReport BuildExecutionReport(
        Order order,
        string execId,
        char execType,
        char ordStatus,
        TimeProvider? timeProvider = null)
    {
        var now = (timeProvider ?? TimeProvider.System).GetUtcNow().UtcDateTime;

        var report = new FixMessage.ExecutionReport(
            new OrderID(Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()),
            new ExecID(execId),
            new ExecType(execType),
            new OrdStatus(ordStatus),
            new QuickFix.Fields.Symbol(order.Symbol.ToString()),
            new Side(order.Side.ToFixValue()),
            new LeavesQty(0),
            new CumQty(order.Quantity.Value),
            new AvgPx(order.Price.Value))
        {
            ClOrdID = new ClOrdID(order.ClOrdId),
            OrderQty = new OrderQty(order.Quantity.Value),
            Price = new Price(order.Price.Value),
            TransactTime = new TransactTime(now)
        };

        return report;
    }
}