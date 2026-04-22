using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FixTrading.OrderGenerator.Api.Integration.Tests.Fixtures;
using FixTrading.OrderProcessing.Application.Contracts;
using FixTrading.OrderProcessing.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FixTrading.OrderGenerator.Api.Integration.Tests.Controllers;

[Collection(nameof(IntegrationTestCollection))]
public class OrdersControllerTests : IntegrationTestBase
{
    public OrdersControllerTests(OrderGeneratorApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateOrder_WithValidRequest_ShouldReturnOkWithAcceptedStatus()
    {
        // Arrange
        var request = OrderRequestBuilder.Valid();

        // Act
        var response = await PostOrderAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await ReadResponseAsync<SendOrderResponse>(response);
        result.Should().NotBeNull();
        result!.Status.Should().Be(OrderStatus.Accepted);
        result.IsSuccess.Should().BeTrue();
        result.ClOrdId.Should().NotBeNullOrEmpty();
        result.ClOrdId.Should().StartWith("ORD-");
        result.Timestamp.Should().NotBeNull();
    }

    [Theory]
    [InlineData("PETR4", "Buy", 100, 35.50)]
    [InlineData("VALE3", "Sell", 500, 72.00)]
    [InlineData("VIIA4", "Buy", 1000, 2.50)]
    public async Task CreateOrder_WithDifferentValidInputs_ShouldReturnOk(
        string symbol, string side, int quantity, decimal price)
    {
        // Arrange
        var request = new SendOrderRequest(symbol, side, quantity, price);

        // Act
        var response = await PostOrderAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateOrder_ShouldCallFixClientWithCorrectOrder()
    {
        // Arrange
        var request = new OrderRequestBuilder()
            .WithSymbol("PETR4")
            .WithSide("Buy")
            .WithQuantity(100)
            .WithPrice(35.50m)
            .Build();

        // Act
        await PostOrderAsync(request);

        // Assert
        await Factory.MockFixClient.Received(1).SendNewOrderAsync(
            Arg.Is<Order>(o =>
                o.Symbol.ToString() == "PETR4" &&
                o.Side.ToString() == "Buy" &&
                o.Quantity.Value == 100 &&
                o.Price.Value == 35.50m),
            Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task CreateOrder_WithInvalidSymbol_ShouldReturnBadRequest()
    {
        // Arrange
        var request = OrderRequestBuilder.WithInvalidSymbol();

        // Act
        var response = await PostOrderAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await ReadResponseAsync<ValidationProblemDetails>(response);
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("Symbol");
    }

    [Fact]
    public async Task CreateOrder_WithInvalidSide_ShouldReturnBadRequest()
    {
        // Arrange
        var request = OrderRequestBuilder.WithInvalidSide();

        // Act
        var response = await PostOrderAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await ReadResponseAsync<ValidationProblemDetails>(response);
        problem!.Errors.Should().ContainKey("Side");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100_000)]
    public async Task CreateOrder_WithInvalidQuantity_ShouldReturnBadRequest(int quantity)
    {
        // Arrange
        var request = new OrderRequestBuilder().WithQuantity(quantity).Build();

        // Act
        var response = await PostOrderAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await ReadResponseAsync<ValidationProblemDetails>(response);
        problem!.Errors.Should().ContainKey("Quantity");
    }

    [Theory]
    [InlineData(0.00)]
    [InlineData(-0.01)]
    [InlineData(1000.00)]
    public async Task CreateOrder_WithInvalidPrice_ShouldReturnBadRequest(decimal price)
    {
        // Arrange
        var request = new OrderRequestBuilder().WithPrice(price).Build();

        // Act
        var response = await PostOrderAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await ReadResponseAsync<ValidationProblemDetails>(response);
        problem!.Errors.Should().ContainKey("Price");
    }

    [Theory]
    [InlineData(10.001)]
    [InlineData(50.555)]
    public async Task CreateOrder_WithInvalidTickSize_ShouldReturnBadRequest(decimal price)
    {
        // Arrange
        var request = new OrderRequestBuilder().WithPrice(price).Build();

        // Act
        var response = await PostOrderAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_WithMultipleInvalidFields_ShouldReturnAllErrors()
    {
        // Arrange
        var request = OrderRequestBuilder.AllInvalid();

        // Act
        var response = await PostOrderAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await ReadResponseAsync<ValidationProblemDetails>(response);
        problem!.Errors.Keys.Should().Contain(["Symbol", "Side", "Quantity", "Price"]);
    }

    [Fact]
    public async Task CreateOrder_WithValidationError_ShouldNotCallFixClient()
    {
        // Arrange
        var request = OrderRequestBuilder.WithInvalidSymbol();

        // Act
        await PostOrderAsync(request);

        // Assert
        await Factory.MockFixClient.DidNotReceive()
            .SendNewOrderAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region POST /api/orders - FIX Failures

    [Fact]
    public async Task CreateOrder_WhenFixClientFails_ShouldReturnBadRequestWithFailedStatus()
    {
        // Arrange
        Factory.SetupFixClientFailure();
        var request = OrderRequestBuilder.Valid();

        // Act
        var response = await PostOrderAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await ReadResponseAsync<SendOrderResponse>(response);
        result!.Status.Should().Be(OrderStatus.Failed);
        result.IsSuccess.Should().BeFalse();
        result.ClOrdId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetSymbols_ShouldReturnAllValidSymbols()
    {
        // Act
        var response = await Client.GetAsync("/api/orders/symbols");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var symbols = await response.Content.ReadFromJsonAsync<string[]>();
        symbols.Should().Contain(["PETR4", "VALE3", "VIIA4"]);
    }


    [Fact]
    public async Task GetSides_ShouldReturnAllValidSides()
    {
        // Act
        var response = await Client.GetAsync("/api/orders/sides");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var sides = await response.Content.ReadFromJsonAsync<string[]>();
        sides.Should().Contain(["Buy", "Sell"]);
    }
}