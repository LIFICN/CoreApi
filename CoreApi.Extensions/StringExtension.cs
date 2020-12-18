using System;

namespace CoreApi.Extensions
{
    public static class StringExtension
    {
        public static string Append(this string oldStr, string newStr)
        {
            int sumLen = oldStr.Length + newStr.Length;
            return string.Create(sumLen, (oldStr, newStr), (buffer, val) =>
            {
                val.oldStr.AsSpan().CopyTo(buffer.Slice(0, oldStr.Length));
                val.newStr.AsSpan().CopyTo(buffer.Slice(oldStr.Length, newStr.Length));
            });
        }
    }
}
