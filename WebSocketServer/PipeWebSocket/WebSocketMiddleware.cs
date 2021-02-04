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
        public int ReceiveBufferSize { get => StaticOptions.Options.ReceiveBufferSize; }
        public int MaxPackageLength { get => StaticOptions.Options.MaxPackageLength; }

        private readonly RequestDelegate _next;

        public WebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = StaticOptions.Path;

            if (context.Request.Path.StartsWithSegments(path))
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                    StaticOptions.Options.OnOpen?.Invoke(context, webSocket);
                    await ProcessAsync(context, webSocket).ConfigureAwait(false);
                }
                else
                    context.Response.StatusCode = 400;
            }
            else
                await _next?.Invoke(context);
        }

        private async ValueTask ProcessAsync(HttpContext context, WebSocket webSocket)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            try
            {
                while (!webSocket.CloseStatus.HasValue)
                {
                    await ProcessLineAsync(context, webSocket, tokenSource.Token).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                StaticOptions.Options.OnException?.Invoke(context, webSocket, ex);
            }
            finally
            {
                tokenSource.Cancel();
                StaticOptions.Options.OnClose?.Invoke(context, webSocket);
            }
        }

        private async ValueTask ProcessLineAsync(HttpContext context, WebSocket webSocket, CancellationToken token)
        {
            using ResizePoolBuffer<byte> bufferPool = new ResizePoolBuffer<byte>(ReceiveBufferSize);

            while (true)
            {
                var result = await webSocket.ReceiveAsync(bufferPool.GetMemory(ReceiveBufferSize), token).ConfigureAwait(false);
                bufferPool.Advance(result.Count);

                if (bufferPool.Count > MaxPackageLength) throw new Exception("the packet length exceeds the maximum packet length");

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    var msgResult = new WebSocketMsgResult(webSocket, result.MessageType, result.EndOfMessage);
                    StaticOptions.Options.OnMessage?.Invoke(context, msgResult, null, bufferPool.WrittenMemory);
                }

                if (result.EndOfMessage)
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var msgResult = new WebSocketMsgResult(webSocket, result.MessageType, result.EndOfMessage);
                        StaticOptions.Options.OnMessage?.Invoke(context, msgResult, Encoding.UTF8.GetString(bufferPool.WrittenSpan), null);
                    }

                    break;
                }
            }
        }
    }
}