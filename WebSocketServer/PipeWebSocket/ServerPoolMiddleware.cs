using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PipeWebSocket
{
    internal class ServerPoolMiddleware
    {
        public int ReceiveBufferSize { get => Config.ReceiveBufferSize; }
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ServerPoolMiddleware(RequestDelegate next, ILogger<ServerPoolMiddleware> logger)
        {
            _next = next;
            _logger = logger ?? throw new Exception($"{nameof(logger)} was null");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = Config.Path;

            if (context.Request.Path.StartsWithSegments(path))
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                    Config.ConfigAction.OnOpen?.Invoke(context, webSocket);
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
                if (_logger != null)
                    _logger.LogError(ex.Message);
            }
            finally
            {
                tokenSource.Cancel();
                Config.ConfigAction.OnClose?.Invoke(context, webSocket);
            }
        }

        private async ValueTask ProcessLineAsync(HttpContext context, WebSocket webSocket, CancellationToken token)
        {
            using PoolArrayBuffer<byte> bufferPool = new PoolArrayBuffer<byte>(ReceiveBufferSize);
            var buffer = new Memory<byte>(bufferPool.Buffer);
            Memory<byte> memory = Memory<byte>.Empty;

            while (true)
            {
                var result = await webSocket.ReceiveAsync(buffer, token).ConfigureAwait(false);
                var bytesReceive = result.Count;
                var receiveMemory = buffer.Slice(0, bytesReceive);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    if (memory.IsEmpty && result.EndOfMessage)  //如果第一个包是完整包，避免拷贝
                        memory = receiveMemory;
                    else if (memory.IsEmpty)                    //如果第一个包是不是完整包，必须拷贝
                        memory = receiveMemory.ToArray();
                    else
                        memory = memory.Append(receiveMemory);  //拼接分包
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    var msgResult = new WebSocketMsgResult(webSocket, result.MessageType, result.EndOfMessage);
                    Config.ConfigAction.OnMessage?.Invoke(context, msgResult, null, receiveMemory);
                }

                if (result.EndOfMessage)
                {
                    if (result.MessageType == WebSocketMessageType.Text && !memory.IsEmpty)
                    {
                        var msgResult = new WebSocketMsgResult(webSocket, result.MessageType, result.EndOfMessage);
                        Config.ConfigAction.OnMessage?.Invoke(context, msgResult, Encoding.UTF8.GetString(memory.Span), null);
                    }

                    break;
                }
            }
        }
    }
}