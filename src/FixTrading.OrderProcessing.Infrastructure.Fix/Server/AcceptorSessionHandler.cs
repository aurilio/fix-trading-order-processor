using System.Threading.Channels;
using FixTrading.OrderProcessing.Domain.Abstractions;
using FixTrading.OrderProcessing.Domain.Entities;
using FixTrading.OrderProcessing.Infrastructure.Fix.Messages;
using Microsoft.Extensions.Logging;
using QuickFix;
using QuickFix.Fields;
using FixMessage = QuickFix.FIX44;

namespace FixTrading.OrderProcessing.Infrastructure.Fix.Server;

// Handler para sessões FIX do Acceptor (servidor).
// Usa Channel para processamento assíncrono sem bloqueio.
public sealed class AcceptorSessionHandler : MessageCracker, IApplication
{
    private readonly IOrderRepository _orderRepository;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger _logger;
    private readonly Channel<OrderProcessingRequest> _orderChannel;

    public event Func<Order, CancellationToken, Task>? OnOrderReceived;

    public AcceptorSessionHandler(
        IOrderRepository orderRepository,
        TimeProvider timeProvider,
        ILogger logger)
    {
        _orderRepository = orderRepository;
        _timeProvider = timeProvider;
        _logger = logger;
        
        // Channel unbounded para não bloquear o handler FIX
        _orderChannel = Channel.CreateUnbounded<OrderProcessingRequest>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

        // Inicia o processador em background
        _ = ProcessOrdersAsync();
    }

    // Processa ordens de forma assíncrona a partir do Channel.
    private async Task ProcessOrdersAsync()
    {
        await foreach (var request in _orderChannel.Reader.ReadAllAsync())
        {
            try
            {
                await _orderRepository.AddAsync(request.Order, CancellationToken.None);

                if (OnOrderReceived is not null)
                {
                    await OnOrderReceived(request.Order, CancellationToken.None);
                }

                SendExecutionReport(request.Order, request.SessionId, ExecType.NEW, OrdStatus.NEW);

                _logger.LogInformation(
                    "Order processed - {@Order}",
                    new { request.Order.ClOrdId, request.Order.Symbol, request.Order.Side, 
                          Quantity = request.Order.Quantity.Value, Price = request.Order.Price.Value });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order {ClOrdId}", request.Order.ClOrdId);
                SendExecutionReport(request.Order, request.SessionId, ExecType.REJECTED, OrdStatus.REJECTED);
            }
        }
    }

    public void OnCreate(SessionID sessionId)
    {
        _logger.LogInformation("Acceptor session created: {SessionId}", sessionId);
    }

    public void OnLogon(SessionID sessionId)
    {
        _logger.LogInformation("Client logged on: {SessionId}", sessionId);
    }

    public void OnLogout(SessionID sessionId)
    {
        _logger.LogInformation("Client logged out: {SessionId}", sessionId);
    }

    public void ToAdmin(Message message, SessionID sessionId)
    {
        _logger.LogDebug("Acceptor ToAdmin: {MsgType}", message.Header.GetString(35));
    }

    public void FromAdmin(Message message, SessionID sessionId)
    {
        _logger.LogDebug("Acceptor FromAdmin: {MsgType}", message.Header.GetString(35));
    }

    public void ToApp(Message message, SessionID sessionId)
    {
        _logger.LogDebug("Acceptor ToApp: {MsgType}", message.Header.GetString(35));
    }

    public void FromApp(Message message, SessionID sessionId)
    {
        _logger.LogDebug("Acceptor FromApp: {MsgType}", message.Header.GetString(35));
        Crack(message, sessionId);
    }

    public void OnMessage(FixMessage.NewOrderSingle message, SessionID sessionId)
    {
        var clOrdId = FixMessageParser.GetClOrdId(message);
        _logger.LogInformation("NewOrderSingle received - ClOrdId: {ClOrdId}", clOrdId);

        try
        {
            var order = FixMessageParser.ParseNewOrderSingle(message, _timeProvider);

            // Enfileira para processamento assíncrono - NÃO BLOQUEIA
            if (!_orderChannel.Writer.TryWrite(new OrderProcessingRequest(order, sessionId)))
            {
                _logger.LogError("Failed to enqueue order {ClOrdId} - channel full", clOrdId);
                SendReject(message, sessionId, "Server busy, please retry");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing order {ClOrdId}", clOrdId);
            SendReject(message, sessionId, ex.Message);
        }
    }

    private void SendExecutionReport(Order order, SessionID sessionId, char execType, char ordStatus)
    {
        var session = Session.LookupSession(sessionId);
        if (session is null || !session.IsLoggedOn)
        {
            _logger.LogWarning("Cannot send ExecutionReport - session {SessionId} not connected", sessionId);
            return;
        }

        var execId = $"EXEC-{_timeProvider.GetUtcNow():yyyyMMddHHmmssfff}";
        var report = FixMessageBuilder.BuildExecutionReport(order, execId, execType, ordStatus);
        
        var sent = Session.SendToTarget(report, sessionId);
        if (!sent)
        {
            _logger.LogWarning("Failed to send ExecutionReport for {ClOrdId}", order.ClOrdId);
        }
    }

    private void SendReject(FixMessage.NewOrderSingle message, SessionID sessionId, string reason)
    {
        var session = Session.LookupSession(sessionId);
        if (session is null || !session.IsLoggedOn)
        {
            _logger.LogWarning("Cannot send Reject - session {SessionId} not connected", sessionId);
            return;
        }

        var reject = new FixMessage.BusinessMessageReject(
            new RefMsgType("D"),
            new BusinessRejectReason(BusinessRejectReason.OTHER))
        {
            Text = new Text(reason)
        };

        if (message.IsSetField(11))
        {
            reject.BusinessRejectRefID = new BusinessRejectRefID(message.ClOrdID.Value);
        }

        Session.SendToTarget(reject, sessionId);
    }

    // Processamento assíncrono de ordem.
    private sealed record OrderProcessingRequest(Order Order, SessionID SessionId);
}