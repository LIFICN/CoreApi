using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using PipeWebSocket;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WebSocketServer;

public class Program
{
    public static readonly ConcurrentDictionary<string, WebSocket> WebSocketCache = new ConcurrentDictionary<string, WebSocket>();
    public static IConfiguration Configuration { get; private set; }

    public static void Main(string[] args)
    {
        CreateWebApplication(args);
    }

    private static void CreateWebApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.UseKestrel((kestrelOptions) =>
        {
            //TCP最大连接数
            kestrelOptions.Limits.MaxConcurrentConnections = 20000;
            //TCP升级至其他连接最大连接数,例如:websocket
            kestrelOptions.Limits.MaxConcurrentUpgradedConnections = 20000;
        });

        BuildServices(builder.Services);
        var app = builder.Build();

        //builder.WebHost.UseUrls(new string[] { "http://*:8080", "http://*:8081" });

        BuildApp(app);
        app.Run();
    }

    private static void BuildServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddLogging(builder =>
        {
            builder.AddConsole()
                   .AddFilter<ConsoleLoggerProvider>(category: null, level: LogLevel.Information);
        });
    }

    private static void BuildApp(WebApplication app)
    {
        //app.UseHttpsRedirection(); //if you want to use wss, please open
        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UseWebSockets(new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromSeconds(120)
        });

        app.UseWebSocketServerMiddleware(options =>
        {
            options.ReceiveBufferSize = 4 * 1024;  //缓冲区大小，可适当增大缓冲区，建议不超过8k
            options.MaxPackageLength = 1 * 1024 * 1024;
            options.Path = "/";

            options.OnOpen = (context, websocket) =>
            {
                string id = context.Connection.Id;
                WebSocketCache.TryAdd(id, websocket);
                Console.WriteLine($"{id} opened");
                return Task.CompletedTask;
            };
            options.OnMessage = async (context, webSocketMsgResult, message, file) =>
            {
                var msgType = webSocketMsgResult.MessageType;

                if (msgType == WebSocketMessageType.Text)
                {
                    Console.WriteLine($"received {context.Connection.Id}: {message}");
                    await webSocketMsgResult.WebSocket.SendAsync(message).ConfigureAwait(false);
                }
                else if (msgType == WebSocketMessageType.Binary)
                {
                    Console.WriteLine($"file received {file.Length / 1024}KB");
                    if (webSocketMsgResult.EndOfMessage) Console.WriteLine("file received completed");
                }
            };
            options.OnClose = (context, webSocket) =>
            {
                string id = context.Connection.Id;
                WebSocketCache.TryRemove(id, out _);
                Console.WriteLine($"{id} closed");
                return Task.CompletedTask;
            };
            options.OnException = (context, webSocket, ex) =>
            {
                string id = context.Connection.Id;
                Console.WriteLine($"{id} {ex.Message}");
                return Task.CompletedTask;
            };
        });
    }
}
