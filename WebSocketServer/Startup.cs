using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PipeWebSocket;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;

namespace WebSocketServer
{
    public class Startup
    {
        public static ConcurrentDictionary<string, WebSocket> dic = new ConcurrentDictionary<string, WebSocket>();
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            //services.AddLogging(builder =>
            //{
            //    builder.AddConsole()
            //           .AddDebug()
            //           .AddFilter<ConsoleLoggerProvider>(category: null, level: LogLevel.Information)
            //           .AddFilter<DebugLoggerProvider>(category: null, level: LogLevel.Debug);
            //});
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

            int bufferSize = 4 * 1024;  //缓冲区大小，适当增大缓冲区可减少拷贝，最好不超过8k

            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = bufferSize
            });

            app.UseWebSocketServerMiddleware("/", bufferSize, builder =>
            {
                builder.OnOpen = (context, websocket) =>
                {
                    string id = context.Connection.Id;
                    dic.TryAdd(id, websocket);
                    Console.WriteLine($"{id} Opened");
                };
                builder.OnMessage = async (context, webSocketMsgResult, message, file) =>
                {
                    var msgType = webSocketMsgResult.MessageType;

                    if (msgType == WebSocketMessageType.Text)
                    {
                        Console.WriteLine($"Received {context.Connection.Id}: {message}");
                        await webSocketMsgResult.WebSocket.SendAsync(message).ConfigureAwait(false);
                    }
                    else if (msgType == WebSocketMessageType.Binary)
                    {
                        using FileStream fileStream = new FileStream("your file path", FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                        await fileStream.WriteAsync(file).ConfigureAwait(false);
                        await fileStream.FlushAsync().ConfigureAwait(false);

                        if (webSocketMsgResult.EndOfMessage)
                            Console.WriteLine("file received completed");
                    }
                };
                builder.OnClose = (context, webSocket) =>
                {
                    string id = context.Connection.Id;
                    dic.TryRemove(id, out _);
                    Console.WriteLine($"{id} Closed");
                };
            });
        }
    }
}
