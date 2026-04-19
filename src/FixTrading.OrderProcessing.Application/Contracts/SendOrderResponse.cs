namespace FixTrading.OrderProcessing.Application.Contracts;

public sealed record SendOrderResponse
{
    public required string? ClOrdId { get; init; }
    public required OrderStatus Status { get; init; }
    public required string Message { get; init; }
    public DateTime? Timestamp { get; init; }

    public bool IsSuccess => Status == OrderStatus.Accepted;

    public static SendOrderResponse Accepted(string clOrdId, DateTime timestamp) => new()
    {
        ClOrdId = clOrdId,
        Status = OrderStatus.Accepted,
        Message = "Order sent successfully",
        Timestamp = timestamp
    };

    public static SendOrderResponse Rejected(string message, string? clOrdId = null) => new()
    {
        ClOrdId = clOrdId,
        Status = OrderStatus.Rejected,
        Message = message,
        Timestamp = null
    };

    public static SendOrderResponse Failed(string message, string? clOrdId = null) => new()
    {
        ClOrdId = clOrdId,
        Status = OrderStatus.Failed,
        Message = message,
        Timestamp = null
    };
}