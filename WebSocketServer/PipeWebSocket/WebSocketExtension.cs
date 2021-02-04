using Microsoft.AspNetCore.Builder;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PipeWebSocket
{
    public static class WebSocketExtension
    {
        public static async ValueTask SendAsync(this WebSocket webSocket, string msg)
        {
            var msgByte = new Memory<byte>(Encoding.UTF8.GetBytes(msg));
            await webSocket.SendAsync(msgByte, WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
        }

        public static async ValueTask CloseAsync(this WebSocket webSocket)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, webSocket.CloseStatusDescription, CancellationToken.None).ConfigureAwait(false);
        }

        public static void UseWebSocketServerMiddleware(this IApplicationBuilder app, string path, Action<PipeWebSocketOptions> action)
        {
            if (action == null) throw new NullReferenceException("action was null");

            var options = new PipeWebSocketOptions();
            action(options);

            if (!string.IsNullOrWhiteSpace(path)) StaticOptions.Path = path;
            if (options.MaxPackageLength <= 0) throw new Exception("MaxPackageLength <= 0");
            if (options.ReceiveBufferSize <= 0) throw new Exception("ReceiveBufferSize <= 0");

            StaticOptions.Options = options;
            app.UseMiddleware<WebSocketMiddleware>();
        }
    }
}