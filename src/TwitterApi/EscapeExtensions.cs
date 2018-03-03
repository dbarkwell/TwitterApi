using System;

namespace TwitterApi
{
    public static class EscapeExtensions
    {
        public static string ToEscapedString(this Uri uri)
        {
           return Uri.EscapeDataString(uri.ToString());
        }

        public static string ToEscapedString(this string str)
        {
            return Uri.EscapeDataString(str);
        }

        public static bool IsEscapedString(this Uri uri)
        {
            var unescaped = uri.ToEscapedString();
            return unescaped != uri.ToString();
        }

        public static bool IsEscapedString(this string str)
        {
            var unescaped = str.ToEscapedString();
            return unescaped != str;
        }
    }
}
