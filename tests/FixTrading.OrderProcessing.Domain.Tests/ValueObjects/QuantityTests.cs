using FluentAssertions;
using FixTrading.OrderProcessing.Domain.Exceptions;
using FixTrading.OrderProcessing.Domain.ValueObjects;

namespace FixTrading.OrderProcessing.Domain.Tests.ValueObjects;

public class QuantityTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(5000)]
    [InlineData(99_999)]
    public void Create_WithValidQuantity_ShouldReturnQuantity(int value)
    {
        // Act
        var quantity = Quantity.Create(value);

        // Assert
        quantity.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithQuantityBelowMinimum_ShouldThrowDomainException(int value)
    {
        // Act
        var act = () => Quantity.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"*{Quantity.MinValue}*{Quantity.MaxValue}*")
            .Where(e => e.Code == "INVALID_QUANTITY");
    }

    [Theory]
    [InlineData(100_000)]
    [InlineData(150_000)]
    [InlineData(int.MaxValue)]
    public void Create_WithQuantityAboveMaximum_ShouldThrowDomainException(int value)
    {
        // Act
        var act = () => Quantity.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"*{Quantity.MinValue}*{Quantity.MaxValue}*")
            .Where(e => e.Code == "INVALID_QUANTITY");
    }

    [Fact]
    public void ImplicitConversion_ToInt_ShouldReturnValue()
    {
        // Arrange
        var quantity = Quantity.Create(500);

        // Act
        int result = quantity;

        // Assert
        result.Should().Be(500);
    }

    [Fact]
    public void ToString_ShouldReturnStringValue()
    {
        // Arrange
        var quantity = Quantity.Create(1234);

        // Act
        var result = quantity.ToString();

        // Assert
        result.Should().Be("1234");
    }

    [Fact]
    public void Equality_SameQuantities_ShouldBeEqual()
    {
        // Arrange
        var qty1 = Quantity.Create(100);
        var qty2 = Quantity.Create(100);

        // Assert
        qty1.Should().Be(qty2);
        (qty1 == qty2).Should().BeTrue();
    }

    [Fact]
    public void Create_AtBoundaryValues_ShouldSucceed()
    {
        // Act
        var minQty = Quantity.Create(Quantity.MinValue);
        var maxQty = Quantity.Create(Quantity.MaxValue);

        // Assert
        minQty.Value.Should().Be(Quantity.MinValue);
        maxQty.Value.Should().Be(Quantity.MaxValue);
    }
}