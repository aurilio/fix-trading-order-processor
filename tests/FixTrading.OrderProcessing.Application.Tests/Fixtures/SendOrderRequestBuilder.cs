using Bogus;
using FixTrading.OrderProcessing.Application.Contracts;
using FixTrading.OrderProcessing.Domain.ValueObjects;

namespace FixTrading.OrderProcessing.Application.Tests.Fixtures;

public sealed class SendOrderRequestBuilder
{
    private static readonly string[] ValidSymbols = ["PETR4", "VALE3", "VIIA4"];
    private static readonly string[] ValidSides = ["Buy", "Sell"];

    private readonly Faker _faker = new();

    private string _symbol;
    private string _side;
    private int _quantity;
    private decimal _price;

    public SendOrderRequestBuilder()
    {
        _symbol = _faker.PickRandom(ValidSymbols);
        _side = _faker.PickRandom(ValidSides);
        _quantity = _faker.Random.Int(Quantity.MinValue, 1000);
        _price = Math.Round(_faker.Random.Decimal(Price.MinValue, 100m), 2);
    }

    public SendOrderRequestBuilder WithSymbol(string symbol)
    {
        _symbol = symbol;
        return this;
    }

    public SendOrderRequestBuilder WithSide(string side)
    {
        _side = side;
        return this;
    }

    public SendOrderRequestBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public SendOrderRequestBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public SendOrderRequest Build() => new(_symbol, _side, _quantity, _price);

    public static SendOrderRequest CreateValid() => new SendOrderRequestBuilder().Build();

    public static SendOrderRequest CreateInvalid() => new SendOrderRequestBuilder()
        .WithSymbol("INVALID")
        .WithSide("INVALID")
        .WithQuantity(0)
        .WithPrice(0m)
        .Build();
}