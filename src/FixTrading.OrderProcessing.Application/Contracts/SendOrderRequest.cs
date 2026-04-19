namespace FixTrading.OrderProcessing.Application.Contracts;

public sealed record SendOrderRequest(
    string Symbol,
    string Side,
    int Quantity,
    decimal Price);