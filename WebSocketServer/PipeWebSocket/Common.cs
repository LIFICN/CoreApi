using Microsoft.AspNetCore.Http;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace PipeWebSocket
{
    public class PipeWebSocketOptions
    {
        public string Path { get; set; } = "/";
        public int ReceiveBufferSize { get; set; } = 4 * 1024;
        public int MaxPackageLength { get; set; } = 1 * 1024 * 1024;
        public Func<HttpContext, WebSocket, Task> OnOpen { get; set; }
        public Func<HttpContext, WebSocketMsgResult, string, ReadOnlyMemory<byte>, Task> OnMessage { get; set; }
        public Func<HttpContext, WebSocket, Task> OnClose { get; set; }
        public Func<HttpContext, WebSocket, Exception, Task> OnException { get; set; }
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