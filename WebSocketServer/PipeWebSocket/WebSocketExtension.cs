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

        public static void UseWebSocketServerMiddleware(this IApplicationBuilder app, string path, int bufferSize, Action<WebSocketConfigAction> action)
        {
            if (action == null) throw new NullReferenceException("action was null");

            if (!string.IsNullOrWhiteSpace(path)) Config.Path = path;

            if (bufferSize > 0) Config.ReceiveBufferSize = bufferSize;

            Config.ConfigAction = new WebSocketConfigAction();
            action(Config.ConfigAction);

            app.UseMiddleware<WebSocketMiddleware>();
        }
    }
}