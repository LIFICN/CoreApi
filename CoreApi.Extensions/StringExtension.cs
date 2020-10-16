using System;

namespace CoreApi.Extensions
{
    public static class StringExtension
    {
        public static string[] ToArray(this string str, char separator = ',')
        {
            return str.Split(separator);
        }

        public static string Slice(this string str,int start,int length=0)
        {
            if (length > 0)
               return str.AsSpan(start,length).ToString();

            return str.AsSpan(start).ToString();
        }
    }
}
