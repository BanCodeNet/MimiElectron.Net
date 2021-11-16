using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Net.Sockets;
using DwFramework.Core;
using DwFramework.Web;

namespace MiniElectron.Core
{
    public sealed class Bridge
    {
        private readonly string _sockPath;
        private byte[] _buffer;

        public int BufferSize = 1024;

        public Func<CmdMessage, object> OnReceive { get; set; }

        public Bridge(WebService webService)
        {
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

        }

        private void OnSendHandler(WebSocketConnection connection, OnSendEventArgs args)
        {

        }

        public async Task<CmdMessage> SendAsync(string topic, dynamic body = null, bool isCallback = false)
        {
            var request = new CmdMessage()
            {
                RequestId = Guid.NewGuid().ToString(),
                Topic = topic,
                Body = body,
                IsCallback = isCallback
            };
            CmdMessage response = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var stream = new NamedPipeClientStream(".", _sockPath.Split('\\').Last(), PipeDirection.InOut);
                stream.Connect(2000);
                await stream.WriteAsync(request.ToJsonBytes());
                await stream.FlushAsync();
                if (isCallback)
                {
                    var data = new List<byte>();
                    while (true)
                    {
                        _buffer = new byte[BufferSize];
                        var len = await stream.ReadAsync(_buffer);
                        if (len <= 0) break;
                        data.AddRange(_buffer[..len]);
                        if (len < BufferSize || Encoding.UTF8.GetString(data.TakeLast(2).ToArray()) == "\r\n") break;
                    }
                    if (data.Count > 0)
                    {
                        response = request with { Body = data.ToArray().FromJsonBytes<dynamic>() };
                    }
                }
            }
            else if (
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            )
            {
                using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                socket.Connect(new UnixDomainSocketEndPoint(_sockPath));
                await socket.SendAsync(new ReadOnlyMemory<byte>(request.ToJsonBytes()), SocketFlags.None);
                if (isCallback)
                {
                    var data = new List<byte>();
                    while (true)
                    {
                        _buffer = new byte[BufferSize];
                        var len = await socket.ReceiveAsync(_buffer, SocketFlags.None);
                        if (len <= 0) break;
                        data.AddRange(_buffer[..len]);
                        if (len < BufferSize || Encoding.UTF8.GetString(data.TakeLast(2).ToArray()) == "\r\n") break;
                    }
                    if (data.Count > 0)
                    {
                        response = request with { Body = data.ToArray().FromJsonBytes<dynamic>() };
                    }
                }
            }
            return response;
        }
    }
}