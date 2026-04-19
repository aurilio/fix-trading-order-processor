using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Domain.Enums;
using FixMessage = QuickFix.FIX44;

namespace FixTrading.OrderProcessing.Infrastructure.Fix.Messages;

public static class FixMessageParser
{
    public static Order ParseNewOrderSingle(FixMessage.NewOrderSingle message, TimeProvider? timeProvider = null)
    {
        var symbolStr = message.Symbol.Value;
        var sideChar = message.Side.Value;
        var quantity = (int)message.OrderQty.Value;
        var price = message.Price.Value;

        var symbol = SymbolExtensions.FromString(symbolStr);
        var side = OrderSideExtensions.FromFixValue(sideChar);

        return Order.Create(symbol, side, quantity, price, timeProvider);
    }

    public static string GetClOrdId(FixMessage.NewOrderSingle message)
        => message.ClOrdID.Value;
}