using System;
using System.Buffers;

namespace PipeWebSocket;

public class ResizeMemory<T> : IDisposable
{
    private T[] _buff;
    private int _count = 0;
    public int Capacity { get => _buff.Length; }
    public int Count { get => _count; }
    public ReadOnlyMemory<T> Memory { get => _buff.AsMemory(0, _count); }
    public ResizeMemory(int minLength) => _buff = ArrayPool<T>.Shared.Rent(minLength);

    public Memory<T> GetMemory(int minLength)
    {
        CheckSize(minLength);
        return _count == 0 ? _buff : _buff.AsMemory(_count);
    }

    public void Write(ReadOnlyMemory<T> value)
    {
        if (value.IsEmpty) return;
        value.CopyTo(GetMemory(value.Length));
        _count += value.Length;
    }

    private void CheckSize(int minLength)
    {
        if (minLength <= 0) throw new ArgumentOutOfRangeException(nameof(minLength));

        if (minLength + _count <= Capacity) return;
        var newSize = checked(_count + minLength);  //检测是否超出int最大值,超出即抛出异常
        var newBuff = ArrayPool<T>.Shared.Rent(newSize);
        Array.Copy(_buff, newBuff, _count);
        ArrayPool<T>.Shared.Return(_buff);
        _buff = newBuff;
    }

    public void Advance(int count)
    {
        if (count < 0 || _count > _buff.Length - count) throw new ArgumentOutOfRangeException(nameof(count));
        _count += count;
    }

    public void Dispose() => ArrayPool<T>.Shared.Return(_buff);
}
