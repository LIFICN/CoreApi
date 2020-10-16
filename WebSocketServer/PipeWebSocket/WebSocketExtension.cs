using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Reflection.Emit;
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
            await webSocket.SendAsync(msgByte, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static async ValueTask CloseAsync(this WebSocket webSocket)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, webSocket.CloseStatusDescription, CancellationToken.None);
        }

        public static void UseWebSocketServerMiddleware(this IApplicationBuilder app, string path, int bufferSize, Action<WebSocketConfigAction> action)
        {
            if (action == null)
                throw new NullReferenceException("action was null");

            if (!string.IsNullOrWhiteSpace(path))
                Config.Path = path;

            if (bufferSize > 0)
                Config.ReceiveBufferSize = bufferSize;

            Config.ConfigAction = new WebSocketConfigAction();
            action(Config.ConfigAction);

            app.UseMiddleware<ServerPoolMiddleware>();
        }
    }

    internal static class ListExtension
    {
        static class ArrayAccessor<T>
        {
            public static Func<List<T>, T[]> Getter;

            static ArrayAccessor()
            {
                var dm = new DynamicMethod("get", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(T[]), new Type[] { typeof(List<T>) }, typeof(ArrayAccessor<T>), true);
                var il = dm.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0); // Load List<T> argument
                il.Emit(OpCodes.Ldfld, typeof(List<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)); // Replace argument by field
                il.Emit(OpCodes.Ret); // Return field
                Getter = (Func<List<T>, T[]>)dm.CreateDelegate(typeof(Func<List<T>, T[]>));
            }
        }

        public static Span<T> GetInternalSpan<T>(this List<T> list)
        {
            return new Span<T>(ArrayAccessor<T>.Getter(list));
        }
    }

    public static class MemoryExtension
    {
        public static Memory<T> Append<T>(this Memory<T> self, in Memory<T> memory)
        {
            Memory<T> newMemory = new T[self.Length + memory.Length];
            self.CopyTo(newMemory.Slice(0, self.Length));
            memory.CopyTo(newMemory.Slice(self.Length, memory.Length));
            return newMemory;
        }
    }
}