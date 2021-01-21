using Microsoft.AspNetCore.Http;
using System;
using System.Net.WebSockets;

namespace PipeWebSocket
{
    internal class StaticOptions
    {
        public static string Path { get; set; } = "/";
        public static PipeWebSocketOptions Options { get; set; }
    }

    public class PipeWebSocketOptions
    {
        public int ReceiveBufferSize { get; set; } = 4 * 1024;
        public int MaxPackageLength { get; set; } = 1 * 1024 * 1024;
        public Action<HttpContext, WebSocket> OnOpen { get; set; }
        public Action<HttpContext, WebSocketMsgResult, string, ReadOnlyMemory<byte>> OnMessage { get; set; }
        public Action<HttpContext, WebSocket> OnClose { get; set; }
    }

    public class WebSocketMsgResult
    {
        public WebSocketMessageType MessageType { get; }
        public bool EndOfMessage { get; }
        public WebSocket WebSocket { get; }

        public WebSocketMsgResult(WebSocket webSocket, WebSocketMessageType messageType, bool endOfMessage)
        {
            WebSocket = webSocket;
            MessageType = messageType;
            EndOfMessage = endOfMessage;
        }
    }
}