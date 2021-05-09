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
        public static PipeWebSocketOptions GlobalOptions { get; internal set; }

        public static async ValueTask SendAsync(this WebSocket webSocket, string msg)
        {
            var msgByte = new Memory<byte>(Encoding.UTF8.GetBytes(msg));
            await webSocket.SendAsync(msgByte, WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
        }

        public static async ValueTask CloseAsync(this WebSocket webSocket)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, webSocket.CloseStatusDescription, CancellationToken.None).ConfigureAwait(false);
        }

        public static void UseWebSocketServerMiddleware(this IApplicationBuilder app, Action<PipeWebSocketOptions> action)
        {
            if (action == null) throw new NullReferenceException("action was null");

            GlobalOptions = new PipeWebSocketOptions();
            action(GlobalOptions);

            if (string.IsNullOrWhiteSpace(GlobalOptions.Path)) throw new Exception("path was empty");
            if (GlobalOptions.MaxPackageLength <= 0) throw new Exception("MaxPackageLength <= 0");
            if (GlobalOptions.ReceiveBufferSize <= 0) throw new Exception("ReceiveBufferSize <= 0");
            app.UseMiddleware<WebSocketMiddleware>();
        }
    }
}