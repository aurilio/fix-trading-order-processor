using FixTrading.OrderAccumulator.Worker;
using FixTrading.OrderProcessing.Application.Services;
using FixTrading.OrderProcessing.Infrastructure.Fix;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddFixAcceptor(builder.Configuration);

builder.Services.AddSingleton<ExposureApplicationService>();

builder.Services.AddHostedService<FixAcceptorWorker>();

var host = builder.Build();
host.Run();