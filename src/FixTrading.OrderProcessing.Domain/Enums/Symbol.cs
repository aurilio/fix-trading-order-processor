using FixTrading.OrderProcessing.Domain.Exceptions;

namespace FixTrading.OrderProcessing.Domain.Enums;

public enum Symbol
{
    PETR4 = 1,
    VALE3 = 2,
    VIIA4 = 3
}

public static class SymbolExtensions
{
    private static readonly Dictionary<string, Symbol> SymbolMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PETR4"] = Symbol.PETR4,
        ["VALE3"] = Symbol.VALE3,
        ["VIIA4"] = Symbol.VIIA4
    };

    public static readonly IReadOnlyCollection<string> ValidSymbols = SymbolMap.Keys.ToArray();

    public static Symbol FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Symbol is required.", "INVALID_SYMBOL");
        }

        if (!SymbolMap.TryGetValue(value, out var symbol))
        {
            throw new DomainException(
                $"Invalid symbol: {value}. Valid symbols: {string.Join(", ", ValidSymbols)}",
                "INVALID_SYMBOL");
        }

        return symbol;
    }

    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) && SymbolMap.ContainsKey(value);
}