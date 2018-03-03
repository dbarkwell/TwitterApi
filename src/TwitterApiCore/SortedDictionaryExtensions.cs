using System.Collections.Generic;
using System.Text;

namespace TwitterApi
{
    public static class SortedDictionaryExtensions
    {
        public static string ToWebString(this SortedDictionary<string, string> source)
        {
            var body = new StringBuilder();
            foreach (var requestParameter in source)
            {
                if (body.Length > 0)
                    body.Append("&");

                body.Append(requestParameter.Key);
                body.Append("=");
                body.Append(requestParameter.Value.IsEscapedString() ? 
                    requestParameter.Value : 
                    requestParameter.Value.ToEscapedString());
            }

            return body.ToString();
        }
    }
}
