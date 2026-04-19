using FixTrading.OrderProcessing.Application.Contracts;
using FixTrading.OrderProcessing.Domain.Enums;
using FixTrading.OrderProcessing.Domain.ValueObjects;
using FluentValidation;

namespace FixTrading.OrderProcessing.Application.Validators;

public sealed class SendOrderRequestValidator : AbstractValidator<SendOrderRequest>
{
    public SendOrderRequestValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Symbol is required.")
            .Must(SymbolExtensions.IsValid)
            .WithMessage(x => $"Invalid symbol: {x.Symbol}. Valid: {string.Join(", ", SymbolExtensions.ValidSymbols)}");

        RuleFor(x => x.Side)
            .NotEmpty().WithMessage("Side is required.")
            .Must(OrderSideExtensions.IsValid)
            .WithMessage(x => $"Invalid side: {x.Side}. Valid: {string.Join(", ", OrderSideExtensions.ValidSides)}");

        RuleFor(x => x.Quantity)
            .InclusiveBetween(Quantity.MinValue, Quantity.MaxValue)
            .WithMessage($"Quantity must be between {Quantity.MinValue} and {Quantity.MaxValue}.");

        RuleFor(x => x.Price)
            .InclusiveBetween(Price.MinValue, Price.MaxValue)
            .WithMessage($"Price must be between {Price.MinValue} and {Price.MaxValue}.")
            .Must(BeValidTickSize)
            .WithMessage($"Price must be a multiple of {Price.TickSize}.");
    }

    private static bool BeValidTickSize(decimal price) =>
        decimal.Round(price, 2) == price;
}