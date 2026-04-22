using FluentAssertions;
using FixTrading.OrderProcessing.Domain.Enums;
using FixTrading.OrderProcessing.Domain.Exceptions;

namespace FixTrading.OrderProcessing.Domain.Tests.Enums;

public class OrderSideTests
{
    [Theory]
    [InlineData("Buy", OrderSide.Buy)]
    [InlineData("Sell", OrderSide.Sell)]
    [InlineData("BUY", OrderSide.Buy)]
    [InlineData("SELL", OrderSide.Sell)]
    [InlineData("buy", OrderSide.Buy)]
    [InlineData("sell", OrderSide.Sell)]
    [InlineData("Compra", OrderSide.Buy)]
    [InlineData("Venda", OrderSide.Sell)]
    [InlineData("B", OrderSide.Buy)]
    [InlineData("S", OrderSide.Sell)]
    public void FromString_WithValidValue_ShouldReturnOrderSide(string input, OrderSide expected)
    {
        // Act
        var result = OrderSideExtensions.FromString(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromString_WithNullOrEmpty_ShouldThrowDomainException(string? input)
    {
        // Act
        var act = () => OrderSideExtensions.FromString(input!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*required*")
            .Where(e => e.Code == "INVALID_SIDE");
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("Long")]
    [InlineData("Short")]
    [InlineData("X")]
    public void FromString_WithInvalidValue_ShouldThrowDomainException(string input)
    {
        // Act
        var act = () => OrderSideExtensions.FromString(input);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"*{input}*")
            .Where(e => e.Code == "INVALID_SIDE");
    }

    [Theory]
    [InlineData('1', OrderSide.Buy)]
    [InlineData('2', OrderSide.Sell)]
    public void FromFixValue_WithValidChar_ShouldReturnOrderSide(char fixValue, OrderSide expected)
    {
        // Act
        var result = OrderSideExtensions.FromFixValue(fixValue);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData('0')]
    [InlineData('3')]
    [InlineData('A')]
    [InlineData('B')]
    public void FromFixValue_WithInvalidChar_ShouldThrowDomainException(char fixValue)
    {
        // Act
        var act = () => OrderSideExtensions.FromFixValue(fixValue);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"*{fixValue}*")
            .Where(e => e.Code == "INVALID_SIDE");
    }

    [Theory]
    [InlineData(OrderSide.Buy, '1')]
    [InlineData(OrderSide.Sell, '2')]
    public void ToFixValue_ShouldReturnCorrectChar(OrderSide side, char expected)
    {
        // Act
        var result = side.ToFixValue();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void RoundTrip_FromStringToFixValue_ShouldBeConsistent()
    {
        // Arrange
        var buySide = OrderSideExtensions.FromString("Buy");
        var sellSide = OrderSideExtensions.FromString("Sell");

        // Act & Assert
        buySide.ToFixValue().Should().Be('1');
        sellSide.ToFixValue().Should().Be('2');

        OrderSideExtensions.FromFixValue('1').Should().Be(buySide);
        OrderSideExtensions.FromFixValue('2').Should().Be(sellSide);
    }
}