using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Net.Sockets;
using DwFramework.Core;

namespace MiniElectron.Core
{
    public sealed class IpcBridge
    {
        private readonly string _ipcPath;
        private byte[] _buffer;

        public int BufferSize = 1024;

        public Func<IpcMessage, object> OnReceive { get; set; }

        public IpcBridge(string ipcPath)
        {
            _ipcPath = ipcPath;
        }

        public async Task<IpcMessage> Send(string topic, dynamic body = null, bool waitResponse = false)
        {
            using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            socket.Connect(new UnixDomainSocketEndPoint(_ipcPath));
            var request = new IpcMessage()
            {
                RequestId = Guid.NewGuid().ToString(),
                Topic = topic,
                Body = body
            };
            socket.Send(request.ToJsonBytes());
            IpcMessage response = null;
            if (waitResponse)
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
                    response = data.ToArray().FromJsonBytes<IpcMessage>();
                }
            }
            return response;
        }
    }
}