using FluentAssertions;
using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Domain.Enums;
using FixTrading.OrderProcessing.Domain.Exceptions;
using FixTrading.OrderProcessing.Domain.Tests.Fixtures;
using FixTrading.OrderProcessing.Domain.ValueObjects;

namespace FixTrading.OrderProcessing.Domain.Tests.Entities;

public class OrderTests
{
    private readonly FakeTimeProvider _timeProvider;
    private readonly OrderFaker _orderFaker;

    public OrderTests()
    {
        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 4, 21, 10, 30, 0, TimeSpan.Zero));
        _orderFaker = new OrderFaker();
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateOrder()
    {
        // Arrange
        var symbol = Symbol.PETR4;
        var side = OrderSide.Buy;
        var quantity = 100;
        var price = 35.50m;

        // Act
        var order = Order.Create(symbol, side, quantity, price, _timeProvider);

        // Assert
        order.Should().NotBeNull();
        order.Symbol.Should().Be(symbol);
        order.Side.Should().Be(side);
        order.Quantity.Value.Should().Be(quantity);
        order.Price.Value.Should().Be(price);
        order.CreatedAtUtc.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueClOrdId()
    {
        // Arrange & Act
        var order1 = Order.Create(Symbol.PETR4, OrderSide.Buy, 100, 10.00m, _timeProvider);
        var order2 = Order.Create(Symbol.VALE3, OrderSide.Sell, 200, 20.00m, _timeProvider);

        // Assert
        order1.ClOrdId.Should().NotBe(order2.ClOrdId);
    }

    [Fact]
    public void Create_ClOrdId_ShouldFollowExpectedFormat()
    {
        // Act
        var order = Order.Create(Symbol.PETR4, OrderSide.Buy, 100, 10.00m, _timeProvider);

        // Assert
        order.ClOrdId.Should().StartWith("ORD-");
        order.ClOrdId.Should().Contain("20260421");
        order.ClOrdId.Should().MatchRegex(@"^ORD-\d{17}-[A-Z0-9]{8}$");
    }

    [Fact]
    public void Create_WithInvalidQuantity_ShouldThrowDomainException()
    {
        // Act
        var act = () => Order.Create(Symbol.PETR4, OrderSide.Buy, 0, 10.00m);

        // Assert
        act.Should().Throw<DomainException>()
            .Where(e => e.Code == "INVALID_QUANTITY");
    }

    [Fact]
    public void Create_WithInvalidPrice_ShouldThrowDomainException()
    {
        // Act
        var act = () => Order.Create(Symbol.PETR4, OrderSide.Buy, 100, 0m);

        // Assert
        act.Should().Throw<DomainException>()
            .Where(e => e.Code == "INVALID_PRICE");
    }

    [Fact]
    public void Create_WithoutTimeProvider_ShouldUseSystemTime()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var order = Order.Create(Symbol.PETR4, OrderSide.Buy, 100, 10.00m);

        // Assert
        var after = DateTime.UtcNow;
        order.CreatedAtUtc.Should().BeOnOrAfter(before);
        order.CreatedAtUtc.Should().BeOnOrBefore(after);
    }

    [Theory]
    [InlineData(100, 10.00, 1000.00)]
    [InlineData(50, 25.50, 1275.00)]
    [InlineData(1, 0.01, 0.01)]
    [InlineData(1000, 100.00, 100_000.00)]
    public void CalculateFinancialValue_ShouldReturnCorrectValue(
        int quantity, decimal price, decimal expectedValue)
    {
        // Arrange
        var order = Order.Create(Symbol.PETR4, OrderSide.Buy, quantity, price, _timeProvider);

        // Act
        var result = order.CalculateFinancialValue();

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(OrderSide.Buy, 100, 10.00, 1000.00)]
    [InlineData(OrderSide.Buy, 50, 20.00, 1000.00)]
    [InlineData(OrderSide.Sell, 100, 10.00, -1000.00)]
    [InlineData(OrderSide.Sell, 50, 20.00, -1000.00)]
    public void CalculateExposure_ShouldReturnCorrectSignBasedOnSide(
        OrderSide side, int quantity, decimal price, decimal expectedExposure)
    {
        // Arrange
        var order = Order.Create(Symbol.PETR4, side, quantity, price, _timeProvider);

        // Act
        var result = order.CalculateExposure();

        // Assert
        result.Should().Be(expectedExposure);
    }

    [Fact]
    public void CalculateExposure_BuyOrder_ShouldReturnPositiveValue()
    {
        // Arrange
        var order = _orderFaker.WithSide(OrderSide.Buy);

        // Act
        var exposure = order.CalculateExposure();

        // Assert
        exposure.Should().BePositive();
    }

    [Fact]
    public void CalculateExposure_SellOrder_ShouldReturnNegativeValue()
    {
        // Arrange
        var order = _orderFaker.WithSide(OrderSide.Sell);

        // Act
        var exposure = order.CalculateExposure();

        // Assert
        exposure.Should().BeNegative();
    }

    [Theory]
    [InlineData(Symbol.PETR4)]
    [InlineData(Symbol.VALE3)]
    [InlineData(Symbol.VIIA4)]
    public void Create_WithAllValidSymbols_ShouldSucceed(Symbol symbol)
    {
        // Act
        var order = Order.Create(symbol, OrderSide.Buy, 100, 10.00m, _timeProvider);

        // Assert
        order.Symbol.Should().Be(symbol);
    }
}