using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using PipeWebSocket;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;

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
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            int bufferSize = 4 * 1024;  //缓冲区大小，可适当增大缓冲区，建议不超过8k

            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = bufferSize
            });

            app.UseWebSocketServerMiddleware("/", options =>
            {
                options.ReceiveBufferSize = bufferSize;
                options.MaxPackageLength = 1 * 1024 * 1024;

                options.OnOpen = (context, websocket) =>
                {
                    string id = context.Connection.Id;
                    WebSocketCache.TryAdd(id, websocket);
                    Console.WriteLine($"{id} opened");
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
                        var fullName = $"{AppContext.BaseDirectory}{context.Request.Query["fileName"]}";
                        using FileStream fileStream = new FileStream(fullName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                        await fileStream.WriteAsync(file).ConfigureAwait(false);
                        await fileStream.FlushAsync().ConfigureAwait(false);

                        if (webSocketMsgResult.EndOfMessage)
                            Console.WriteLine("file received completed");
                    }
                };
                options.OnClose = (context, webSocket) =>
                {
                    string id = context.Connection.Id;
                    WebSocketCache.TryRemove(id, out _);
                    Console.WriteLine($"{id} closed");
                };
            });
        }
    }
}
