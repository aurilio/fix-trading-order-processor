using FixTrading.OrderProcessing.Domain.Exceptions;

namespace FixTrading.OrderProcessing.Domain.Enums;

// Mapeamento conforme especificação FIX (Tag 54 - Side)
public enum OrderSide
{
    Buy = 1,
    Sell = 2
}

public static class OrderSideExtensions
{
    private static readonly Dictionary<string, OrderSide> SideMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Buy"] = OrderSide.Buy,
        ["Sell"] = OrderSide.Sell,
        ["Compra"] = OrderSide.Buy,
        ["Venda"] = OrderSide.Sell,
        ["B"] = OrderSide.Buy,
        ["S"] = OrderSide.Sell
    };

    public static readonly IReadOnlyCollection<string> ValidSides = ["Buy", "Sell", "Compra", "Venda"];

    public static OrderSide FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Side is required.", "INVALID_SIDE");
        }

        if (!SideMap.TryGetValue(value, out var side))
        {
            throw new DomainException(
                $"Invalid side: {value}. Valid values: {string.Join(", ", ValidSides)}",
                "INVALID_SIDE");
        }

        return side;
    }

    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) && SideMap.ContainsKey(value);

    public static char ToFixValue(this OrderSide side) => side switch
    {
        OrderSide.Buy => '1',
        OrderSide.Sell => '2',
        _ => throw new DomainException($"Unknown side: {side}", "INVALID_SIDE")
    };
}