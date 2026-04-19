using FixTrading.OrderProcessing.Application.Abstractions;
using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Infrastructure.Fix.Configuration;
using FixTrading.OrderProcessing.Infrastructure.Fix.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;

namespace FixTrading.OrderProcessing.Infrastructure.Fix.Client;

// Cliente FIX Initiator - envia ordens para o Acceptor.
public sealed class FixInitiatorClient : IFixClient, IDisposable
{
    private readonly FixSettings _settings;
    private readonly ILogger<FixInitiatorClient> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly InitiatorSessionHandler _sessionHandler;
    private SocketInitiator? _initiator;
    private SessionID? _sessionId;

    public FixInitiatorClient(
        IOptions<FixSettings> settings,
        TimeProvider timeProvider,
        ILogger<FixInitiatorClient> logger)
    {
        _settings = settings.Value;
        _timeProvider = timeProvider;
        _logger = logger;
        _sessionHandler = new InitiatorSessionHandler(logger);
    }

    public bool IsConnected 
        => _sessionId is not null && Session.LookupSession(_sessionId)?.IsLoggedOn == true;

    public void Start()
    {
        var settingsConfig = CreateSettings();
        var storeFactory = new FileStoreFactory(settingsConfig);
        var logFactory = new ScreenLogFactory(settingsConfig);

        _initiator = new SocketInitiator(_sessionHandler, storeFactory, settingsConfig, logFactory);
        _initiator.Start();

        _sessionId = _initiator.GetSessionIDs().FirstOrDefault();
        _logger.LogInformation("FIX Initiator started. SessionID: {SessionId}", _sessionId);
    }

    public void Stop()
    {
        _initiator?.Stop();
        _logger.LogInformation("FIX Initiator stopped");
    }

    public Task<bool> SendNewOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        // 3.3 - Validação de sessão melhorada
        if (_sessionId is null)
        {
            _logger.LogWarning("Cannot send order {ClOrdId} - session not initialized", order.ClOrdId);
            return Task.FromResult(false);
        }

        var session = Session.LookupSession(_sessionId);
        if (session is null)
        {
            _logger.LogWarning("Cannot send order {ClOrdId} - session not found", order.ClOrdId);
            return Task.FromResult(false);
        }

        if (!session.IsLoggedOn)
        {
            _logger.LogWarning("Cannot send order {ClOrdId} - session not logged on", order.ClOrdId);
            return Task.FromResult(false);
        }

        try
        {
            var message = FixMessageBuilder.BuildNewOrderSingle(order);
            var sent = Session.SendToTarget(message, _sessionId);

            if (sent)
            {
                _logger.LogInformation("Order {ClOrdId} sent via FIX", order.ClOrdId);
            }
            else
            {
                _logger.LogWarning("Failed to send order {ClOrdId} via FIX - SendToTarget returned false", order.ClOrdId);
            }

            return Task.FromResult(sent);
        }
        catch (SessionNotFound ex)
        {
            _logger.LogError(ex, "Session not found when sending order {ClOrdId}", order.ClOrdId);
            return Task.FromResult(false);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid FIX operation when sending order {ClOrdId}", order.ClOrdId);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending order {ClOrdId} via FIX", order.ClOrdId);
            return Task.FromResult(false);
        }
    }

    private SessionSettings CreateSettings()
    {
        var configString = $@"
                            [DEFAULT]
                            ConnectionType=initiator
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
                            SenderCompID={_settings.SenderCompId}
                            TargetCompID={_settings.TargetCompId}
                            SocketConnectHost={_settings.Host}
                            SocketConnectPort={_settings.Port}
                            ";

        using var reader = new StringReader(configString);
        return new SessionSettings(reader);
    }

    public void Dispose()
    {
        Stop();
        _initiator?.Dispose();
    }
}