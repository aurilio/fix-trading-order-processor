using FluentAssertions;
using FixTrading.OrderProcessing.Domain.Exceptions;
using FixTrading.OrderProcessing.Domain.ValueObjects;

namespace FixTrading.OrderProcessing.Domain.Tests.ValueObjects;

public class PriceTests
{
    [Theory]
    [InlineData(0.01)]
    [InlineData(1.00)]
    [InlineData(50.55)]
    [InlineData(100.00)]
    [InlineData(999.99)]
    public void Create_WithValidPrice_ShouldReturnPrice(decimal value)
    {
        // Act
        var price = Price.Create(value);

        // Assert
        price.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(0.00)]
    [InlineData(-0.01)]
    [InlineData(-100.00)]
    public void Create_WithPriceBelowMinimum_ShouldThrowDomainException(decimal value)
    {
        // Act
        var act = () => Price.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"*{Price.MinValue}*{Price.MaxValue}*")
            .Where(e => e.Code == "INVALID_PRICE");
    }

    [Theory]
    [InlineData(1000.00)]
    [InlineData(1500.00)]
    [InlineData(9999.99)]
    public void Create_WithPriceAboveMaximum_ShouldThrowDomainException(decimal value)
    {
        // Act
        var act = () => Price.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"*{Price.MinValue}*{Price.MaxValue}*")
            .Where(e => e.Code == "INVALID_PRICE");
    }

    [Theory]
    [InlineData(10.001)]
    [InlineData(50.555)]
    [InlineData(100.123)]
    public void Create_WithInvalidTickSize_ShouldThrowDomainException(decimal value)
    {
        // Act
        var act = () => Price.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"*{Price.TickSize}*")
            .Where(e => e.Code == "INVALID_PRICE_FORMAT");
    }

    [Fact]
    public void ImplicitConversion_ToDecimal_ShouldReturnValue()
    {
        // Arrange
        var price = Price.Create(99.99m);

        // Act
        decimal result = price;

        // Assert
        result.Should().Be(99.99m);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedValue()
    {
        // Arrange
        var price = Price.Create(10.50m);

        // Act
        var result = price.ToString();

        // Assert
        result.Should().Be("10.50");
    }

    [Fact]
    public void Equality_SamePrices_ShouldBeEqual()
    {
        // Arrange
        var price1 = Price.Create(50.00m);
        var price2 = Price.Create(50.00m);

        // Assert
        price1.Should().Be(price2);
        (price1 == price2).Should().BeTrue();
    }

    [Fact]
    public void Create_AtBoundaryValues_ShouldSucceed()
    {
        // Act
        var minPrice = Price.Create(Price.MinValue);
        var maxPrice = Price.Create(Price.MaxValue);

        // Assert
        minPrice.Value.Should().Be(Price.MinValue);
        maxPrice.Value.Should().Be(Price.MaxValue);
    }
}