using FixTrading.OrderProcessing.Application.Contracts;

namespace FixTrading.OrderProcessing.Application.Abstractions;

public interface IExposureApplicationService
{
    Task<ExposureResponse> GetExposureAsync(CancellationToken cancellationToken = default);
}