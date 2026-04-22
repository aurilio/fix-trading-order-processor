using FluentAssertions;
using FixTrading.OrderProcessing.Domain.Enums;
using FixTrading.OrderProcessing.Domain.Exceptions;

namespace FixTrading.OrderProcessing.Domain.Tests.Enums;

public class SymbolTests
{
    [Theory]
    [InlineData("PETR4", Symbol.PETR4)]
    [InlineData("VALE3", Symbol.VALE3)]
    [InlineData("VIIA4", Symbol.VIIA4)]
    [InlineData("petr4", Symbol.PETR4)]
    [InlineData("vale3", Symbol.VALE3)]
    [InlineData("Petr4", Symbol.PETR4)]
    public void FromString_WithValidSymbol_ShouldReturnSymbol(string input, Symbol expected)
    {
        // Act
        var result = SymbolExtensions.FromString(input);

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
        var act = () => SymbolExtensions.FromString(input!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*required*")
            .Where(e => e.Code == "INVALID_SYMBOL");
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("AAPL")]
    [InlineData("MSFT")]
    [InlineData("PETR3")]
    public void FromString_WithInvalidSymbol_ShouldThrowDomainException(string input)
    {
        // Act
        var act = () => SymbolExtensions.FromString(input);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"*{input}*")
            .Where(e => e.Code == "INVALID_SYMBOL");
    }

    [Theory]
    [InlineData("PETR4", true)]
    [InlineData("VALE3", true)]
    [InlineData("VIIA4", true)]
    [InlineData("petr4", true)]
    [InlineData("INVALID", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValid_ShouldReturnExpectedResult(string? input, bool expected)
    {
        // Act
        var result = SymbolExtensions.IsValid(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ValidSymbols_ShouldContainAllSymbols()
    {
        // Assert
        SymbolExtensions.ValidSymbols.Should().Contain("PETR4");
        SymbolExtensions.ValidSymbols.Should().Contain("VALE3");
        SymbolExtensions.ValidSymbols.Should().Contain("VIIA4");
        SymbolExtensions.ValidSymbols.Should().HaveCount(3);
    }
}