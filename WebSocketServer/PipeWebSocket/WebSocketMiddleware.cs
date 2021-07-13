using Microsoft.AspNetCore.Http;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PipeWebSocket
{
    internal class WebSocketMiddleware
    {
        public PipeWebSocketOptions WebSocketOptionsEx { get => WebSocketExtension.GlobalOptions; }
        private readonly RequestDelegate _next;

        public WebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(WebSocketOptionsEx.Path))
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                    await ProcessAsync(context, webSocket).ConfigureAwait(false);
                }
                else
                    context.Response.StatusCode = 400;
            }
            else
                await _next(context);
        }

        private async ValueTask ProcessAsync(HttpContext context, WebSocket webSocket)
        {
            try
            {
                await WebSocketOptionsEx.OnOpen?.Invoke(context, webSocket);

                while (!webSocket.CloseStatus.HasValue)
                {
                    await ProcessLineAsync(context, webSocket).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await WebSocketOptionsEx.OnException?.Invoke(context, webSocket, ex);
            }
            finally
            {
                await WebSocketOptionsEx.OnClose?.Invoke(context, webSocket);
            }
        }

        private async ValueTask ProcessLineAsync(HttpContext context, WebSocket webSocket)
        {
            using ResizeMemory<byte> bufferPool = new ResizeMemory<byte>(WebSocketOptionsEx.ReceiveBufferSize);

            while (true)
            {
                var buffer = bufferPool.GetMemory(WebSocketOptionsEx.ReceiveBufferSize);
                var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);
                bufferPool.Advance(result.Count);

                if (bufferPool.Count > WebSocketOptionsEx.MaxPackageLength)
                    throw new Exception("the packet length exceeds the maximum packet length");

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    var msgResult = new WebSocketMsgResult(webSocket, result.MessageType, result.EndOfMessage);
                    await WebSocketOptionsEx.OnMessage?.Invoke(context, msgResult, null, bufferPool.Memory);
                }

                if (result.EndOfMessage)
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var msgRes = new WebSocketMsgResult(webSocket, result.MessageType, result.EndOfMessage);
                        await WebSocketOptionsEx.OnMessage?.Invoke(context, msgRes, Encoding.UTF8.GetString(bufferPool.Memory.Span), null);
                    }

                    break;
                }
            }
        }
    }
}