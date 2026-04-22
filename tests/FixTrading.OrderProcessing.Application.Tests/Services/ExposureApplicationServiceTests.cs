using FluentAssertions;
using FixTrading.OrderProcessing.Application.Services;
using FixTrading.OrderProcessing.Application.Tests.Fixtures;
using FixTrading.OrderProcessing.Domain.Abstractions;
using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FixTrading.OrderProcessing.Application.Tests.Services;

public class ExposureApplicationServiceTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly ExposureApplicationService _sut;

    public ExposureApplicationServiceTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();

        _sut = new ExposureApplicationService(
            _orderRepository,
            NullLogger<ExposureApplicationService>.Instance);
    }

    [Fact]
    public async Task GetExposureAsync_WithNoOrders_ShouldReturnZeroExposure()
    {
        // Arrange
        _orderRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Order>());

        // Act
        var result = await _sut.GetExposureAsync();

        // Assert
        result.TotalExposure.Should().Be(0m);
        result.ExposureBySymbol.Should().BeEmpty();
    }

    [Fact]
    public async Task GetExposureAsync_WithSingleBuyOrder_ShouldReturnPositiveExposure()
    {
        // Arrange
        var order = OrderBuilder.CreateBuyOrder(Symbol.PETR4, 100, 10.00m);
        _orderRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { order });

        // Act
        var result = await _sut.GetExposureAsync();

        // Assert
        result.TotalExposure.Should().Be(1000.00m);
        result.ExposureBySymbol.Should().HaveCount(1);
        result.ExposureBySymbol["PETR4"].Should().Be(1000.00m);
    }

    [Fact]
    public async Task GetExposureAsync_WithSingleSellOrder_ShouldReturnNegativeExposure()
    {
        // Arrange
        var order = OrderBuilder.CreateSellOrder(Symbol.VALE3, 50, 20.00m);
        _orderRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { order });

        // Act
        var result = await _sut.GetExposureAsync();

        // Assert
        result.TotalExposure.Should().Be(-1000.00m);
        result.ExposureBySymbol["VALE3"].Should().Be(-1000.00m);
    }

    [Fact]
    public async Task GetExposureAsync_WithMixedOrders_ShouldCalculateNetExposure()
    {
        // Arrange
        var orders = new[]
        {
            OrderBuilder.CreateBuyOrder(Symbol.PETR4, 100, 10.00m),
            OrderBuilder.CreateSellOrder(Symbol.PETR4, 50, 10.00m),
            OrderBuilder.CreateBuyOrder(Symbol.VALE3, 200, 20.00m),
        };
        _orderRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(orders);

        // Act
        var result = await _sut.GetExposureAsync();

        // Assert
        result.TotalExposure.Should().Be(4500.00m);
        result.ExposureBySymbol["PETR4"].Should().Be(500.00m);
        result.ExposureBySymbol["VALE3"].Should().Be(4000.00m);
    }

    [Fact]
    public async Task GetExposureAsync_WithOffsetOrders_ShouldNetToZero()
    {
        // Arrange
        var orders = new[]
        {
            OrderBuilder.CreateBuyOrder(Symbol.PETR4, 100, 10.00m),
            OrderBuilder.CreateSellOrder(Symbol.PETR4, 100, 10.00m),
        };
        _orderRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(orders);

        // Act
        var result = await _sut.GetExposureAsync();

        // Assert
        result.TotalExposure.Should().Be(0m);
        result.ExposureBySymbol["PETR4"].Should().Be(0m);
    }

    [Fact]
    public async Task GetExposureAsync_WithMultipleSymbols_ShouldGroupBySymbol()
    {
        // Arrange
        var orders = OrderBuilder.CreateMixedOrders();
        _orderRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(orders);

        // Act
        var result = await _sut.GetExposureAsync();

        // Assert
        result.ExposureBySymbol.Should().ContainKey("PETR4");
        result.ExposureBySymbol.Should().ContainKey("VALE3");
        result.ExposureBySymbol.Should().ContainKey("VIIA4");
    }

    [Fact]
    public async Task GetExposureAsync_ShouldCallRepository()
    {
        // Arrange
        _orderRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Order>());

        // Act
        await _sut.GetExposureAsync();

        // Assert
        await _orderRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetExposureAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        _orderRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Order>());

        // Act
        await _sut.GetExposureAsync(cts.Token);

        // Assert
        await _orderRepository.Received(1).GetAllAsync(cts.Token);
    }

    [Fact]
    public async Task GetExposureAsync_ExposureBySymbol_ShouldUseStringKeys()
    {
        // Arrange
        var order = OrderBuilder.CreateBuyOrder(Symbol.PETR4, 100, 10.00m);
        _orderRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { order });

        // Act
        var result = await _sut.GetExposureAsync();

        // Assert
        result.ExposureBySymbol.Keys.Should().AllSatisfy(key =>
            key.Should().BeOfType<string>());
    }

    [Fact]
    public async Task GetExposureAsync_TotalExposure_ShouldMatchSumOfSymbolExposures()
    {
        // Arrange
        var orders = OrderBuilder.CreateMixedOrders();
        _orderRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(orders);

        // Act
        var result = await _sut.GetExposureAsync();

        // Assert
        var sumOfSymbols = result.ExposureBySymbol.Values.Sum();
        result.TotalExposure.Should().Be(sumOfSymbols);
    }
}