using FluentAssertions;
using FixTrading.OrderProcessing.Application.Contracts;
using FixTrading.OrderProcessing.Application.Tests.Fixtures;
using FixTrading.OrderProcessing.Application.Validators;
using FixTrading.OrderProcessing.Domain.ValueObjects;

namespace FixTrading.OrderProcessing.Application.Tests.Validators;

public class SendOrderRequestValidatorTests
{
    private readonly SendOrderRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidRequest_ShouldBeValid()
    {
        // Arrange
        var request = SendOrderRequestBuilder.CreateValid();

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("PETR4")]
    [InlineData("VALE3")]
    [InlineData("VIIA4")]
    [InlineData("petr4")]
    [InlineData("vale3")]
    public async Task Validate_WithValidSymbol_ShouldBeValid(string symbol)
    {
        // Arrange
        var request = new SendOrderRequestBuilder()
            .WithSymbol(symbol)
            .Build();

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("INVALID")]
    [InlineData("AAPL")]
    [InlineData("PETR3")]
    public async Task Validate_WithInvalidSymbol_ShouldHaveError(string symbol)
    {
        // Arrange
        var request = new SendOrderRequestBuilder()
            .WithSymbol(symbol)
            .Build();

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Symbol");
    }

    [Theory]
    [InlineData("Buy")]
    [InlineData("Sell")]
    [InlineData("BUY")]
    [InlineData("SELL")]
    [InlineData("Compra")]
    [InlineData("Venda")]
    public async Task Validate_WithValidSide_ShouldBeValid(string side)
    {
        // Arrange
        var request = new SendOrderRequestBuilder()
            .WithSide(side)
            .Build();

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Long")]
    [InlineData("Short")]
    [InlineData("Invalid")]
    public async Task Validate_WithInvalidSide_ShouldHaveError(string side)
    {
        // Arrange
        var request = new SendOrderRequestBuilder()
            .WithSide(side)
            .Build();

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Side");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(50000)]
    [InlineData(99999)]
    public async Task Validate_WithValidQuantity_ShouldBeValid(int quantity)
    {
        // Arrange
        var request = new SendOrderRequestBuilder()
            .WithQuantity(quantity)
            .Build();

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(100000)]
    [InlineData(999999)]
    public async Task Validate_WithInvalidQuantity_ShouldHaveError(int quantity)
    {
        // Arrange
        var request = new SendOrderRequestBuilder()
            .WithQuantity(quantity)
            .Build();

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1.00)]
    [InlineData(50.55)]
    [InlineData(999.99)]
    public async Task Validate_WithValidPrice_ShouldBeValid(decimal price)
    {
        // Arrange
        var request = new SendOrderRequestBuilder()
            .WithPrice(price)
            .Build();

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0.00)]
    [InlineData(-0.01)]
    [InlineData(1000.00)]
    [InlineData(9999.99)]
    public async Task Validate_WithPriceOutOfRange_ShouldHaveError(decimal price)
    {
        // Arrange
        var request = new SendOrderRequestBuilder()
            .WithPrice(price)
            .Build();

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Theory]
    [InlineData(10.001)]
    [InlineData(50.555)]
    [InlineData(100.123)]
    public async Task Validate_WithInvalidTickSize_ShouldHaveError(decimal price)
    {
        // Arrange
        var request = new SendOrderRequestBuilder()
            .WithPrice(price)
            .Build();

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == "Price" && 
            e.ErrorMessage.Contains(Price.TickSize.ToString()));
    }

    [Fact]
    public async Task Validate_WithMultipleInvalidFields_ShouldReturnAllErrors()
    {
        // Arrange
        var request = SendOrderRequestBuilder.CreateInvalid();

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(4);
        result.Errors.Select(e => e.PropertyName).Should()
            .Contain(["Symbol", "Side", "Quantity", "Price"]);
    }
}