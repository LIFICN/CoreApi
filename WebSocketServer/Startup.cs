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

namespace WebSocketServer
{
    public class Startup
    {
        public static readonly ConcurrentDictionary<string, WebSocket> WebSocketCache = new ConcurrentDictionary<string, WebSocket>();
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddLogging(builder =>
            {
                builder.AddConsole()
                       .AddFilter<ConsoleLoggerProvider>(category: null, level: LogLevel.Information);
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
}
