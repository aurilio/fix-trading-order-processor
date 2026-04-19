using FixTrading.OrderProcessing.Domain.Abstractions;
using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Infrastructure.Fix.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;

namespace FixTrading.OrderProcessing.Infrastructure.Fix.Server;

// Servidor FIX Acceptor - recebe ordens dos Initiators.
public sealed class FixAcceptorServer : IDisposable
{
    private readonly FixSettings _settings;
    private readonly ILogger<FixAcceptorServer> _logger;
    private readonly AcceptorSessionHandler _sessionHandler;
    private ThreadedSocketAcceptor? _acceptor;

    public event Func<Order, CancellationToken, Task>? OnOrderReceived;

    public FixAcceptorServer(
        IOptions<FixSettings> settings,
        IOrderRepository orderRepository,
        TimeProvider timeProvider,
        ILogger<FixAcceptorServer> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _sessionHandler = new AcceptorSessionHandler(orderRepository, timeProvider, logger);
        _sessionHandler.OnOrderReceived += async (order, ct) =>
        {
            if (OnOrderReceived is not null)
            {
                await OnOrderReceived(order, ct);
            }
        };
    }

    public void Start()
    {
        var settingsConfig = CreateSettings();
        var storeFactory = new FileStoreFactory(settingsConfig);
        var logFactory = new ScreenLogFactory(settingsConfig);

        _acceptor = new ThreadedSocketAcceptor(_sessionHandler, storeFactory, settingsConfig, logFactory);
        _acceptor.Start();

        _logger.LogInformation("FIX Acceptor started on port {Port}", _settings.Port);
    }

    public void Stop()
    {
        _acceptor?.Stop();
        _logger.LogInformation("FIX Acceptor stopped");
    }

    private SessionSettings CreateSettings()
    {
        var configString = $@"
                            [DEFAULT]
                            ConnectionType=acceptor
                            ReconnectInterval=5
                            FileStorePath={_settings.FileStorePath}
                            StartTime=00:00:00
                            EndTime=00:00:00
                            UseDataDictionary=Y
                            DataDictionary=FIX44.xml
                            HeartBtInt={_settings.HeartBeatInterval}
                            ResetOnLogon={(_settings.ResetOnLogon ? "Y" : "N")}
                            ResetOnLogout={(_settings.ResetOnLogout ? "Y" : "N")}
                            ResetOnDisconnect={(_settings.ResetOnDisconnect ? "Y" : "N")}

                            [SESSION]
                            BeginString={_settings.BeginString}
                            SenderCompID={_settings.TargetCompId}
                            TargetCompID={_settings.SenderCompId}
                            SocketAcceptPort={_settings.Port}
                            ";

        using var reader = new StringReader(configString);
        return new SessionSettings(reader);
    }

    public void Dispose()
    {
        Stop();
        _acceptor?.Dispose();
    }
}