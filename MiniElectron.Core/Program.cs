using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DwFramework.Core;
using DwFramework.Web;

namespace MiniElectron.Core
{
    public static class Program
    {
        public static async Task Main(params string[] args)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var httpPort)) throw new Exception("缺少参数:httpPort");
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
                services.AddSingleton<Bridge>();
            });
            host.OnHostStarted += provider => { };
            await host.RunAsync();
        }
    }
}