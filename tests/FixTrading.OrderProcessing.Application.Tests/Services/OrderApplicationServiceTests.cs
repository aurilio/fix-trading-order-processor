using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using FixTrading.OrderProcessing.Application.Abstractions;
using FixTrading.OrderProcessing.Application.Contracts;
using FixTrading.OrderProcessing.Application.Services;
using FixTrading.OrderProcessing.Application.Tests.Fixtures;
using FixTrading.OrderProcessing.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace FixTrading.OrderProcessing.Application.Tests.Services;

public class OrderApplicationServiceTests
{
    private readonly IFixClient _fixClient;
    private readonly IValidator<SendOrderRequest> _validator;
    private readonly FakeTimeProvider _timeProvider;
    private readonly OrderApplicationService _sut;

    public OrderApplicationServiceTests()
    {
        _fixClient = Substitute.For<IFixClient>();
        _validator = Substitute.For<IValidator<SendOrderRequest>>();
        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 4, 21, 10, 30, 0, TimeSpan.Zero));

        _sut = new OrderApplicationService(
            _fixClient,
            _timeProvider,
            _validator,
            NullLogger<OrderApplicationService>.Instance);
    }

    [Fact]
    public async Task SendOrderAsync_WithValidRequest_ShouldReturnAccepted()
    {
        // Arrange
        var request = SendOrderRequestBuilder.CreateValid();
        SetupValidatorSuccess();
        _fixClient.SendNewOrderAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.SendOrderAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Status.Should().Be(OrderStatus.Accepted);
        result.ClOrdId.Should().NotBeNullOrEmpty();
        result.ClOrdId.Should().StartWith("ORD-");
        result.Timestamp.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
        result.Message.Should().Contain("success");
    }

    [Fact]
    public async Task SendOrderAsync_WhenValidationFails_ShouldReturnRejected()
    {
        // Arrange
        var request = SendOrderRequestBuilder.CreateInvalid();
        var validationErrors = new List<ValidationFailure>
        {
            new("Symbol", "Invalid symbol"),
            new("Price", "Invalid price")
        };
        SetupValidatorFailure(validationErrors);

        // Act
        var result = await _sut.SendOrderAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(OrderStatus.Rejected);
        result.Message.Should().Contain("Invalid symbol");
        result.Message.Should().Contain("Invalid price");
        result.ClOrdId.Should().BeNull();

        await _fixClient.DidNotReceive()
            .SendNewOrderAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendOrderAsync_WhenFixClientFails_ShouldReturnFailed()
    {
        // Arrange
        var request = SendOrderRequestBuilder.CreateValid();
        SetupValidatorSuccess();
        _fixClient.SendNewOrderAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.SendOrderAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(OrderStatus.Failed);
        result.Message.Should().Contain("Failed to send order");
        result.ClOrdId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SendOrderAsync_ShouldPassOrderToFixClient()
    {
        // Arrange
        var request = new SendOrderRequestBuilder()
            .WithSymbol("PETR4")
            .WithSide("Buy")
            .WithQuantity(100)
            .WithPrice(35.50m)
            .Build();
        SetupValidatorSuccess();
        _fixClient.SendNewOrderAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _sut.SendOrderAsync(request);

        // Assert
        await _fixClient.Received(1).SendNewOrderAsync(
            Arg.Is<Order>(o =>
                o.Symbol.ToString() == "PETR4" &&
                o.Side.ToString() == "Buy" &&
                o.Quantity.Value == 100 &&
                o.Price.Value == 35.50m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendOrderAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var request = SendOrderRequestBuilder.CreateValid();
        var cts = new CancellationTokenSource();
        SetupValidatorSuccess();

        _fixClient.SendNewOrderAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var token = callInfo.Arg<CancellationToken>();
                token.ThrowIfCancellationRequested();
                return true;
            });

        // Act
        await cts.CancelAsync();
        var act = () => _sut.SendOrderAsync(request, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData("PETR4", "Buy")]
    [InlineData("VALE3", "Sell")]
    [InlineData("VIIA4", "Buy")]
    public async Task SendOrderAsync_WithDifferentSymbolsAndSides_ShouldSucceed(
        string symbol, string side)
    {
        // Arrange
        var request = new SendOrderRequestBuilder()
            .WithSymbol(symbol)
            .WithSide(side)
            .Build();
        SetupValidatorSuccess();
        _fixClient.SendNewOrderAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.SendOrderAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendOrderAsync_ShouldUseTimeProvider()
    {
        // Arrange
        var expectedTime = new DateTimeOffset(2026, 12, 25, 15, 0, 0, TimeSpan.Zero);
        _timeProvider.SetUtcNow(expectedTime);

        var request = SendOrderRequestBuilder.CreateValid();
        SetupValidatorSuccess();
        _fixClient.SendNewOrderAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.SendOrderAsync(request);

        // Assert
        result.Timestamp.Should().Be(expectedTime.UtcDateTime);
    }

    [Fact]
    public async Task SendOrderAsync_EachCall_ShouldGenerateUniqueClOrdId()
    {
        // Arrange
        var request = SendOrderRequestBuilder.CreateValid();
        SetupValidatorSuccess();
        _fixClient.SendNewOrderAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var results = new List<SendOrderResponse>();
        for (int i = 0; i < 10; i++)
        {
            results.Add(await _sut.SendOrderAsync(request));
        }

        // Assert
        var clOrdIds = results.Select(r => r.ClOrdId).ToList();
        clOrdIds.Should().OnlyHaveUniqueItems();
    }

    private void SetupValidatorSuccess()
    {
        _validator.ValidateAsync(Arg.Any<SendOrderRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private void SetupValidatorFailure(List<ValidationFailure> errors)
    {
        _validator.ValidateAsync(Arg.Any<SendOrderRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(errors));
    }
}