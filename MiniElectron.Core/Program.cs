using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using DwFramework.Core;

namespace MiniElectron.Core
{
    public static class Program
    {
        public static async Task Main(params string[] args)
        {
            if (args.Length < 1) throw new Exception("缺少参数:ipcPath");
            var host = new ServiceHost();
            host.OnHostStarted += provider =>
            {
                var endPoint = new UnixDomainSocketEndPoint(args[0]);
                var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                socket.Connect(endPoint);
                socket.Send(Encoding.UTF8.GetBytes("xxxx"));
            };
            await host.RunAsync();
        }
    }
}