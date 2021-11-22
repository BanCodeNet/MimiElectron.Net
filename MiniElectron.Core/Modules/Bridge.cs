using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;
using DwFramework.Core;
using DwFramework.Web;

namespace MiniElectron.Core
{
    [Registerable(isAutoActivate: true)]
    public sealed class Bridge
    {
        private readonly ILogger<Bridge> _logger;
        private byte[] _buffer;
        private WebSocketConnection _shellConnection;

        public int BufferSize { get; set; } = 1024;
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
            _logger.LogDebug(Encoding.UTF8.GetString(args.Data));
            var message = args.Data.FromJsonBytes<Message>();
            if (message == null) return;
            switch (message.Topic)
            {
                case "Register":
                    _shellConnection = connection;
                    _logger.LogInformation($"Shell已连接: {_shellConnection.ID}");
                    break;
            }
        }

        private void OnSendHandler(WebSocketConnection connection, OnSendEventArgs args)
        {

        }

        public async Task<Message> SendAsync(string topic, dynamic body = null, bool isCallback = false)
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
                response = request with { Body = data.ToArray().FromJsonBytes<dynamic>() };
            }
            return response;
        }
    }
}