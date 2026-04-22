using FixTrading.OrderProcessing.Application.Abstractions;
using FixTrading.OrderProcessing.Application.Contracts;
using FixTrading.OrderProcessing.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FixTrading.OrderGenerator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderApplicationService _orderService;
    private readonly IValidator<SendOrderRequest> _validator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderApplicationService orderService,
        IValidator<SendOrderRequest> validator,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _validator = validator;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(SendOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(SendOrderResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] SendOrderRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Received order request: {Symbol} {Side} {Qty}@{Price}",
            request.Symbol, request.Side, request.Quantity, request.Price);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        var response = await _orderService.SendOrderAsync(request, cancellationToken);

        if (!response.IsSuccess)
        {
            _logger.LogWarning("Order {Status}: {Message}", response.Status, response.Message);

            return response.Status == OrderStatus.Rejected
                ? UnprocessableEntity(response)
                : BadRequest(response);
        }

        _logger.LogInformation("Order accepted: {ClOrdId}", response.ClOrdId);
        return Ok(response);
    }

    [HttpGet("symbols")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public IActionResult GetSymbols()
    {
        var symbols = SymbolExtensions.ValidSymbols;
        return Ok(symbols);
    }

    [HttpGet("sides")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public IActionResult GetSides()
    {
        var sides = OrderSideExtensions.ValidSides;
        return Ok(sides);
    }
}