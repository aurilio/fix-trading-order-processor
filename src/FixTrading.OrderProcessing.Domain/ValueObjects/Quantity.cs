using FixTrading.OrderProcessing.Domain.Exceptions;

namespace FixTrading.OrderProcessing.Domain.ValueObjects;

public readonly record struct Quantity
{
    public const int MaxValue = 99_999;
    public const int MinValue = 1;

    public int Value { get; }

    private Quantity(int value) => Value = value;

    public static Quantity Create(int value)
    {
        if (value < MinValue || value > MaxValue)
        {
            throw new DomainException(
                $"Quantity must be between {MinValue} and {MaxValue}. Received: {value}",
                "INVALID_QUANTITY");
        }

        return new Quantity(value);
    }

    public static implicit operator int(Quantity quantity) => quantity.Value;

    public override string ToString() => Value.ToString();
}