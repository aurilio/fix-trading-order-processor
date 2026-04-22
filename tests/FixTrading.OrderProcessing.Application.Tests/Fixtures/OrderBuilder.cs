using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Domain.Enums;

namespace FixTrading.OrderProcessing.Application.Tests.Fixtures;

public static class OrderBuilder
{
    public static Order CreateBuyOrder(Symbol symbol = Symbol.PETR4, int quantity = 100, decimal price = 10.00m)
        => Order.Create(symbol, OrderSide.Buy, quantity, price);

    public static Order CreateSellOrder(Symbol symbol = Symbol.PETR4, int quantity = 100, decimal price = 10.00m)
        => Order.Create(symbol, OrderSide.Sell, quantity, price);

    public static IReadOnlyCollection<Order> CreateMixedOrders()
    {
        return
        [
            CreateBuyOrder(Symbol.PETR4, 100, 10.00m),
            CreateSellOrder(Symbol.PETR4, 50, 10.00m),
            CreateBuyOrder(Symbol.VALE3, 200, 20.00m),
            CreateSellOrder(Symbol.VIIA4, 100, 5.00m),
        ];
    }
}