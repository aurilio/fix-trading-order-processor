using FixTrading.OrderProcessing.Domain.Exceptions;

namespace FixTrading.OrderProcessing.Domain.ValueObjects;

public readonly record struct Price
{
    public const decimal MaxValue = 999.99m;
    public const decimal MinValue = 0.01m;
    public const decimal TickSize = 0.01m;

    public decimal Value { get; }

    private Price(decimal value) => Value = value;

    public static Price Create(decimal value)
    {
        if (value < MinValue || value > MaxValue)
        {
            throw new DomainException(
                $"Price must be between {MinValue} and {MaxValue}. Received: {value}",
                "INVALID_PRICE");
        }

        if (decimal.Round(value, 2) != value)
        {
            throw new DomainException(
                $"Price must be a multiple of {TickSize}. Received: {value}",
                "INVALID_PRICE_FORMAT");
        }

        return new Price(value);
    }

    public static implicit operator decimal(Price price) => price.Value;

    public override string ToString() => Value.ToString("F2");
}