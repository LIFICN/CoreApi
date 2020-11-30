using System;
using System.Buffers;

namespace PipeWebSocket
{
    public class PoolArrayBuffer<T> : IDisposable
    {
        public T[] Buffer { get; }
        public PoolArrayBuffer(int minLength) => Buffer = ArrayPool<T>.Shared.Rent(minLength);
        public void Dispose() => ArrayPool<T>.Shared.Return(Buffer);
    }
}
