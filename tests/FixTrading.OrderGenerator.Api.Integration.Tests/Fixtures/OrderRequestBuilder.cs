using Bogus;
using FixTrading.OrderProcessing.Application.Contracts;
using FixTrading.OrderProcessing.Domain.ValueObjects;

namespace FixTrading.OrderGenerator.Api.Integration.Tests.Fixtures;

public sealed class OrderRequestBuilder
{
    private static readonly string[] ValidSymbols = ["PETR4", "VALE3", "VIIA4"];
    private static readonly string[] ValidSides = ["Buy", "Sell"];

    private readonly Faker _faker = new();

    private string _symbol;
    private string _side;
    private int _quantity;
    private decimal _price;

    public OrderRequestBuilder()
    {
        _symbol = _faker.PickRandom(ValidSymbols);
        _side = _faker.PickRandom(ValidSides);
        _quantity = _faker.Random.Int(Quantity.MinValue, 1000);
        _price = Math.Round(_faker.Random.Decimal(Price.MinValue, 100m), 2);
    }

    public OrderRequestBuilder WithSymbol(string symbol) { _symbol = symbol; return this; }
    public OrderRequestBuilder WithSide(string side) { _side = side; return this; }
    public OrderRequestBuilder WithQuantity(int quantity) { _quantity = quantity; return this; }
    public OrderRequestBuilder WithPrice(decimal price) { _price = price; return this; }

    public SendOrderRequest Build() => new(_symbol, _side, _quantity, _price);

    public static SendOrderRequest Valid() => new OrderRequestBuilder().Build();
    public static SendOrderRequest WithInvalidSymbol() => new OrderRequestBuilder().WithSymbol("INVALID").Build();
    public static SendOrderRequest WithInvalidSide() => new OrderRequestBuilder().WithSide("INVALID").Build();
    public static SendOrderRequest WithInvalidQuantity() => new OrderRequestBuilder().WithQuantity(0).Build();
    public static SendOrderRequest WithInvalidPrice() => new OrderRequestBuilder().WithPrice(0m).Build();
    public static SendOrderRequest AllInvalid() => new("INVALID", "INVALID", 0, 0m);
}