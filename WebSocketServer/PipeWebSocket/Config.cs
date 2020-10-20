using Microsoft.AspNetCore.Http;
using System;
using System.Net.WebSockets;

namespace PipeWebSocket
{
    internal class Config
    {
        public static string Path { get; set; } = "/";
        public static int ReceiveBufferSize { get; set; } = 2 * 1024;
        public static WebSocketConfigAction ConfigAction { get; set; }
    }

    public class WebSocketConfigAction
    {
        public Action<HttpContext, WebSocket> OnOpen { get; set; }
        public Action<HttpContext, WebSocketMsgResult, string, Memory<byte>> OnMessage { get; set; }
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