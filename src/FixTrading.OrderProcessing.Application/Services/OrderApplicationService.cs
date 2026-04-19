using FixTrading.OrderProcessing.Application.Abstractions;
using FixTrading.OrderProcessing.Application.Contracts;
using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Domain.Enums;
using FixTrading.OrderProcessing.Domain.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace FixTrading.OrderProcessing.Application.Services;

public sealed class OrderApplicationService
{
    private readonly IFixClient _fixClient;
    private readonly TimeProvider _timeProvider;
    private readonly IValidator<SendOrderRequest> _validator;
    private readonly ILogger<OrderApplicationService> _logger;

    public OrderApplicationService(
        IFixClient fixClient,
        TimeProvider timeProvider,
        IValidator<SendOrderRequest> validator,
        ILogger<OrderApplicationService> logger)
    {
        _fixClient = fixClient;
        _timeProvider = timeProvider;
        _validator = validator;
        _logger = logger;
    }

    public async Task<SendOrderResponse> SendOrderAsync(
        SendOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Validation failed for order request: {Errors}", errors);
            return SendOrderResponse.Rejected(errors);
        }

        try
        {
            var symbol = SymbolExtensions.FromString(request.Symbol);
            var side = OrderSideExtensions.FromString(request.Side);

            var order = Order.Create(symbol, side, request.Quantity, request.Price, _timeProvider);

            _logger.LogInformation(
                "Sending order {@Order}", new { order.ClOrdId, order.Symbol, order.Side, Quantity = order.Quantity.Value, Price = order.Price.Value });

            var sent = await _fixClient.SendNewOrderAsync(order, cancellationToken);

            if (!sent)
            {
                _logger.LogError("FIX send failed for order {ClOrdId}", order.ClOrdId);
                return SendOrderResponse.Failed("Failed to send order via FIX", order.ClOrdId);
            }

            _logger.LogInformation("Order {ClOrdId} sent successfully", order.ClOrdId);
            return SendOrderResponse.Accepted(order.ClOrdId, order.CreatedAtUtc);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation failed: {Code}", ex.Code);
            return SendOrderResponse.Rejected($"Validation error: {ex.Message}");
        }
    }
}