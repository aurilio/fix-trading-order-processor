using Microsoft.Extensions.Logging;
using QuickFix;
using FixMessage = QuickFix.FIX44;

namespace FixTrading.OrderProcessing.Infrastructure.Fix.Client;

public sealed class InitiatorSessionHandler : MessageCracker, IApplication
{
    private readonly ILogger _logger;

    public event Action<FixMessage.ExecutionReport>? OnExecutionReport;

    public InitiatorSessionHandler(ILogger logger)
    {
        _logger = logger;
    }

    public void OnCreate(SessionID sessionId)
    {
        _logger.LogInformation("Session created: {SessionId}", sessionId);
    }

    public void OnLogon(SessionID sessionId)
    {
        _logger.LogInformation("Logon successful: {SessionId}", sessionId);
    }

    public void OnLogout(SessionID sessionId)
    {
        _logger.LogInformation("Logout: {SessionId}", sessionId);
    }

    public void ToAdmin(Message message, SessionID sessionId)
    {
        _logger.LogDebug("ToAdmin: {MsgType}", message.Header.GetString(35));
    }

    public void FromAdmin(Message message, SessionID sessionId)
    {
        _logger.LogDebug("FromAdmin: {MsgType}", message.Header.GetString(35));
    }

    public void ToApp(Message message, SessionID sessionId)
    {
        _logger.LogDebug("ToApp: {MsgType} - {ClOrdId}",
            message.Header.GetString(35),
            message.IsSetField(11) ? message.GetString(11) : "N/A");
    }

    public void FromApp(Message message, SessionID sessionId)
    {
        _logger.LogDebug("FromApp: {MsgType}", message.Header.GetString(35));
        Crack(message, sessionId);
    }

    public void OnMessage(FixMessage.ExecutionReport message, SessionID sessionId)
    {
        var clOrdId = message.ClOrdID.Value;
        var execType = message.ExecType.Value;
        var ordStatus = message.OrdStatus.Value;

        _logger.LogInformation(
            "ExecutionReport received - ClOrdId: {ClOrdId}, ExecType: {ExecType}, OrdStatus: {OrdStatus}",
            clOrdId, execType, ordStatus);

        OnExecutionReport?.Invoke(message);
    }
}