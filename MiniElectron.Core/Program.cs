using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DwFramework.Core;
using DwFramework.Web;
using System.Text;

namespace MiniElectron.Core
{
    public static class Program
    {
        public static async Task Main(params string[] args)
        {
            if (args.Length < 1 || string.IsNullOrEmpty(args[0])) throw new Exception("缺少参数:ipcPath");
            if (args.Length < 2 || !int.TryParse(args[1], out var httpPort)) throw new Exception("缺少参数:httpPort");
            var host = new ServiceHost();
            host.ConfigureLogging(builder => builder.UserNLog("LogConfig.xml"));
            var configStream = new MemoryStream(new
            {
                Web = new
                {
                    Listens = new[]{
                        new
                        {
                            Scheme = "Http",
                            Port = httpPort
                        }
                    }
                }
            }.ToJsonBytes());
            var config = new ConfigurationBuilder().AddJsonStream(configStream).Build();
            host.ConfigureWeb(config, builder => builder.UseStartup<Startup>(), "Web");
            host.ConfigureServices(services =>
            {
                services.AddSingleton(serviceProvider => new IpcBridge(args[0]));
            });
            host.OnHostStarted += provider =>
            {
                var bridge = provider.GetService<IpcBridge>();
                bridge.OnReceive = request => Console.WriteLine(Encoding.UTF8.GetString(request.Body) + " ======> " + request.Body.Length);
            };
            await host.RunAsync();
        }
    }
}