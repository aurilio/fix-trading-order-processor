using FixTrading.OrderProcessing.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace FixTrading.OrderGenerator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IFixClient _fixClient;
    private readonly TimeProvider _timeProvider;

    public HealthController(
        IFixClient fixClient,
        TimeProvider timeProvider)
    {
        _fixClient = fixClient;
        _timeProvider = timeProvider;
    }

    [HttpGet]
    [ProducesResponseType(typeof(HealthStatus), StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        var status = new HealthStatus
        {
            FixConnected = _fixClient.IsConnected,
            Timestamp = _timeProvider.GetUtcNow().UtcDateTime
        };

        return Ok(status);
    }
}

public record HealthStatus
{
    public bool FixConnected { get; init; }
    public DateTime Timestamp { get; init; }
}