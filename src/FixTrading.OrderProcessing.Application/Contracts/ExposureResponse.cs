namespace FixTrading.OrderProcessing.Application.Contracts;

public sealed record ExposureResponse(
    IReadOnlyDictionary<string, decimal> ExposureBySymbol,
    decimal TotalExposure);