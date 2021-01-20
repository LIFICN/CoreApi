using System;
using System.Buffers;

namespace PipeWebSocket
{
    public class ResizePoolBuffer<T> : IDisposable
    {
        private T[] _buff;
        private int _count = 0;
        public int Capacity { get => _buff.Length; }
        public int Count { get => _count; }
        public ReadOnlySpan<T> WrittenSpan { get => _buff.AsSpan(0, _count); }
        public ReadOnlyMemory<T> WrittenMemory { get => _buff.AsMemory(0, _count); }
        public ResizePoolBuffer(int minLength) => _buff = ArrayPool<T>.Shared.Rent(minLength);

        public Span<T> GetSpan(int minLength)
        {
            CheckSize(minLength);
            return _count == 0 ? _buff : _buff.AsSpan(_count);
        }

        public Memory<T> GetMemory(int minLength)
        {
            CheckSize(minLength);
            return _count == 0 ? _buff : _buff.AsMemory(_count);
        }

        public void Write(T value)
        {
            GetSpan(1)[0] = value;
            _count += 1;
        }

        public void Write(ReadOnlySpan<T> value)
        {
            if (value.IsEmpty) return;
            value.CopyTo(GetSpan(value.Length));
            _count += value.Length;
        }

        private void CheckSize(int minLength)
        {
            if (minLength <= 0) throw new ArgumentOutOfRangeException(nameof(minLength));

            if (minLength + _count > Capacity)
            {
                var newSize = checked(_count + minLength);  //检测是否超出int最大值,超出即抛出异常
                var newBuff = ArrayPool<T>.Shared.Rent(newSize);
                Array.Copy(_buff, newBuff, _count);
                ArrayPool<T>.Shared.Return(_buff);
                _buff = newBuff;
            }
        }

        public void Advance(int count)
        {
            if (count < 0 || _count > _buff.Length - count) throw new ArgumentOutOfRangeException(nameof(count));
            _count += count;
        }

        public void Clear() => _count = 0;
        public void Dispose() => ArrayPool<T>.Shared.Return(_buff);
    }
}
