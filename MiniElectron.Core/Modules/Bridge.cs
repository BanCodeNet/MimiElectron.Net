using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;
using DwFramework.Core;
using DwFramework.Web;

namespace MiniElectron.Core;

[Registerable(lifetime: Lifetime.Singleton, isAutoActivate: true)]
public sealed class Bridge
{
    private readonly ILogger<Bridge> _logger;
    private WebSocketConnection _shellConnection;
    private Dictionary<string, (DateTime ExprieTime, object Response)> _callbackPool = new();

    public Func<Message, object> OnReceive { get; set; }

    public Bridge(ILogger<Bridge> logger, WebService webService)
    {
        _logger = logger;
        webService.OnWebSocketConnect += OnConnectHandler;
        webService.OnWebSocketClose += OnCloseHandler;
        webService.OnWebSocketError += OnErrorHandler;
        webService.OnWebSocketReceive += OnReceiveHandler;
        webService.OnWebSocketSend += OnSendHandler;
    }

    private void OnConnectHandler(WebSocketConnection connection, OnConnectEventArgs args)
    {

    }

    private void OnCloseHandler(WebSocketConnection connection, OnCloceEventArgs args)
    {

    }

    private void OnErrorHandler(WebSocketConnection connection, OnErrorEventArgs args)
    {

    }

    private void OnReceiveHandler(WebSocketConnection connection, OnReceiveEventArgs args)
    {
        Console.WriteLine($"SHELL TO CORE =====> {Encoding.UTF8.GetString(args.Data)}");
        Message message = null;
        dynamic callback = null;
        try
        {
            message = args.Data.FromJsonBytes<Message>();
            if (message == null) return;
            switch (message.Topic)
            {
                case "Callback":
                    if (!_callbackPool.ContainsKey(message.RequestId)) return;
                    _callbackPool[message.RequestId] = (DateTime.Now.AddMilliseconds(3000), message.Body);
                    break;
                case "Register":
                    _shellConnection = connection;
                    _logger.LogInformation($"Shell已连接: {_shellConnection.ID}");
                    break;
                default:
                    callback = OnReceive?.Invoke(message);
                    break;
            }
        }
        catch (Exception ex)
        {
            callback = ex.Message;
        }
        finally
        {
            if (message != null && message.IsCallback)
            {
                var callbackMessage = message with { Body = callback, IsCallback = false };
                connection.SendAsync(message.ToJsonBytes());
            }
        }
    }

    private void OnSendHandler(WebSocketConnection connection, OnSendEventArgs args)
    {

    }

    public async Task<Message> SendAsync(string topic, dynamic body = null, bool isCallback = false, double expireMilliseconds = 3000)
    {
        var request = new Message()
        {
            RequestId = Guid.NewGuid().ToString(),
            Topic = topic,
            Body = body,
            IsCallback = isCallback
        };
        Message response = null;
        await _shellConnection.SendAsync(request.ToJsonBytes());
        if (isCallback)
        {
            _callbackPool[request.RequestId] = (expireMilliseconds > 0 ? DateTime.Now.AddMilliseconds(expireMilliseconds) : default, null);
            while (
                (default == _callbackPool[request.RequestId].ExprieTime || DateTime.Now < _callbackPool[request.RequestId].ExprieTime)
                && _callbackPool[request.RequestId].Response == null
            ) await Task.Delay(500);
            if (_callbackPool[request.RequestId].Response == null) response = request with { Body = "请求超时" };
            else response = request with { Body = _callbackPool[request.RequestId].Response };
            _callbackPool.Remove(request.RequestId);
        }
        return response;
    }
}