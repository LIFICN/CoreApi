using System;

namespace CoreApi.Extensions
{
    public static class ArrayExtension
    {
        public static T[] Slice<T>(this T[] array, int start, int length = 0)
        {
            int len = length > 0 ? length : array.Length - start;
            return new Span<T>(array, start, len).ToArray();
        }

        public static string ToString<T>(this T[] array, char separator)
        {
            return string.Join(separator, array);
        }
    }
}