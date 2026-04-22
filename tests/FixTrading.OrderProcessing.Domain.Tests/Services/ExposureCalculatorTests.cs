using FluentAssertions;
using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Domain.Enums;
using FixTrading.OrderProcessing.Domain.Services;
using FixTrading.OrderProcessing.Domain.Tests.Fixtures;

namespace FixTrading.OrderProcessing.Domain.Tests.Services;

public class ExposureCalculatorTests
{
    private readonly FakeTimeProvider _timeProvider;
    private readonly OrderFaker _orderFaker;

    public ExposureCalculatorTests()
    {
        _timeProvider = new FakeTimeProvider();
        _orderFaker = new OrderFaker();
    }

    [Fact]
    public void CalculateBySymbol_WithEmptyOrders_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var orders = Array.Empty<Order>();

        // Act
        var result = ExposureCalculator.CalculateBySymbol(orders);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void CalculateBySymbol_WithNullOrders_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => ExposureCalculator.CalculateBySymbol(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CalculateBySymbol_WithSingleOrder_ShouldReturnExposure()
    {
        // Arrange
        var order = Order.Create(Symbol.PETR4, OrderSide.Buy, 100, 10.00m, _timeProvider);

        // Act
        var result = ExposureCalculator.CalculateBySymbol([order]);

        // Assert
        result.Should().HaveCount(1);
        result[Symbol.PETR4].Should().Be(1000.00m);
    }

    [Fact]
    public void CalculateBySymbol_WithMultipleSymbols_ShouldGroupCorrectly()
    {
        // Arrange
        var orders = new[]
        {
            Order.Create(Symbol.PETR4, OrderSide.Buy, 100, 10.00m, _timeProvider),
            Order.Create(Symbol.VALE3, OrderSide.Sell, 50, 20.00m, _timeProvider),
            Order.Create(Symbol.PETR4, OrderSide.Sell, 50, 10.00m, _timeProvider),
        };

        // Act
        var result = ExposureCalculator.CalculateBySymbol(orders);

        // Assert
        result.Should().HaveCount(2);
        result[Symbol.PETR4].Should().Be(500.00m);
        result[Symbol.VALE3].Should().Be(-1000.00m);
    }

    [Fact]
    public void CalculateBySymbol_BuyAndSellSameSymbol_ShouldNetOut()
    {
        // Arrange
        var orders = new[]
        {
            Order.Create(Symbol.PETR4, OrderSide.Buy, 100, 10.00m, _timeProvider),
            Order.Create(Symbol.PETR4, OrderSide.Sell, 100, 10.00m, _timeProvider),
        };

        // Act
        var result = ExposureCalculator.CalculateBySymbol(orders);

        // Assert
        result[Symbol.PETR4].Should().Be(0m);
    }

    [Fact]
    public void CalculateTotal_WithEmptyOrders_ShouldReturnZero()
    {
        // Arrange
        var orders = Array.Empty<Order>();

        // Act
        var result = ExposureCalculator.CalculateTotal(orders);

        // Assert
        result.Should().Be(0m);
    }

    [Fact]
    public void CalculateTotal_WithNullOrders_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => ExposureCalculator.CalculateTotal(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CalculateTotal_WithMixedOrders_ShouldSumAllExposures()
    {
        // Arrange
        var orders = new[]
        {
            Order.Create(Symbol.PETR4, OrderSide.Buy, 100, 10.00m, _timeProvider),
            Order.Create(Symbol.VALE3, OrderSide.Sell, 50, 20.00m, _timeProvider),
            Order.Create(Symbol.VIIA4, OrderSide.Buy, 200, 5.00m, _timeProvider),
        };

        // Act
        var result = ExposureCalculator.CalculateTotal(orders);

        // Assert
        result.Should().Be(1000.00m);
    }

    [Fact]
    public void CalculateTotal_OnlyBuyOrders_ShouldReturnPositiveTotal()
    {
        // Arrange
        var orders = Enumerable.Range(1, 5)
            .Select(_ => _orderFaker.WithSide(OrderSide.Buy))
            .ToArray();

        // Act
        var result = ExposureCalculator.CalculateTotal(orders);

        // Assert
        result.Should().BePositive();
    }

    [Fact]
    public void CalculateTotal_OnlySellOrders_ShouldReturnNegativeTotal()
    {
        // Arrange
        var orders = Enumerable.Range(1, 5)
            .Select(_ => _orderFaker.WithSide(OrderSide.Sell))
            .ToArray();

        // Act
        var result = ExposureCalculator.CalculateTotal(orders);

        // Assert
        result.Should().BeNegative();
    }

    [Fact]
    public void CalculateBySymbol_And_CalculateTotal_ShouldBeConsistent()
    {
        // Arrange
        var orders = Enumerable.Range(1, 10)
            .Select(_ => _orderFaker.Generate())
            .ToArray();

        // Act
        var bySymbol = ExposureCalculator.CalculateBySymbol(orders);
        var total = ExposureCalculator.CalculateTotal(orders);

        // Assert
        var sumFromBySymbol = bySymbol.Values.Sum();
        total.Should().Be(sumFromBySymbol);
    }
}