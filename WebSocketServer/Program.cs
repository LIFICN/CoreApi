using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace WebSocketServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(builder =>
                    {
                        //TCP最大连接数
                        builder.Limits.MaxConcurrentConnections = 20000;
                        //TCP升级至其他连接最大连接数,例如:websocket
                        builder.Limits.MaxConcurrentUpgradedConnections = 20000;
                    });

                    //webBuilder.UseUrls(new string[] { "http://localhost:8080", "http://localhost:8081" });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
