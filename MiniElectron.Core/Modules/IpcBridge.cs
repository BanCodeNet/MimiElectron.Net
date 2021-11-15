using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Net.Sockets;
using DwFramework.Core;

namespace MiniElectron.Core
{
    public sealed class IpcBridge
    {
        private readonly string _sockPath;
        private byte[] _buffer;

        public int BufferSize = 1024;

        public Func<IpcMessage, object> OnReceive { get; set; }

        public IpcBridge(string sockPath)
        {
            _sockPath = sockPath;
        }

        public async Task<IpcMessage> SendAsync(string topic, dynamic body = null, bool isCallback = false)
        {
            var request = new IpcMessage()
            {
                RequestId = Guid.NewGuid().ToString(),
                Topic = topic,
                Body = body,
                IsCallback = isCallback
            };
            IpcMessage response = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (var stream = new NamedPipeClientStream(".", _sockPath.Split('\\').Last(), PipeDirection.InOut))
                {
                    stream.Connect(2000);
                    using var writer = new StreamWriter(stream);
                    await writer.WriteLineAsync(request.ToJson());
                    if (isCallback)
                    {
                        using var reader = new StreamReader(stream);
                        response = request with { Body = (await reader.ReadToEndAsync()).FromJson<dynamic>() };
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