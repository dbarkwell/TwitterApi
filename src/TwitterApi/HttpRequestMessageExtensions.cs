using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace TwitterApi
{
    public static class HttpRequestMessageExtensions
    {
        public static void SetTwitterOAuthHeader(this HttpRequestMessage message, string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret)
        {
            var oauthNonce = CreateOAuthNonce();
            var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            var oauthSignature = CreateOAuthSignature(message, consumerKey, consumerKeySecret, accessToken, accessTokenSecret, oauthNonce, timeStamp);

            const string HeaderFormat = "oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                                        "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                                        "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                                        "oauth_version=\"{6}\"";

            var header = string.Format(
                HeaderFormat,
                oauthNonce.ToEscapedString(),
                "HMAC-SHA1".ToEscapedString(),
                timeStamp.ToEscapedString(),
                consumerKey.ToEscapedString(),
                accessToken.ToEscapedString(),
                oauthSignature.ToEscapedString(),
                "1.0".ToEscapedString());

            message.Headers.Authorization = new AuthenticationHeaderValue("OAuth", header);
        }

        private static string CreateOAuthNonce()
        {
            return Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
        }

        private static string CreateOAuthSignature(HttpRequestMessage message, string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret, string nonce, string timestamp)
        {
            var requestParameters = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_nonce", nonce },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", timestamp },
                { "oauth_token", accessToken },
                { "oauth_version", "1.0" }
            };


            var queryStringParams = new Dictionary<string, string>();
            if (message.Method == HttpMethod.Post && message.Content != null)
            {
                var content = message.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                GetRequestParameters(content, ref queryStringParams);
            }

            GetRequestParameters(message.RequestUri, ref queryStringParams);
                
            foreach (var key in queryStringParams.Keys)
            {
                requestParameters.Add(key, queryStringParams[key]);
            }

            var baseString = requestParameters.ToWebString();
            var signatureBaseString =
                $"{message.Method}&{message.RequestUri.GetLeftPart(UriPartial.Path).ToEscapedString()}&{baseString.ToEscapedString()}";
            var compositeKey = $"{consumerKeySecret.ToEscapedString()}&{accessTokenSecret.ToEscapedString()}";

            using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(compositeKey)))
            {
                return Convert.ToBase64String(hasher.ComputeHash(Encoding.ASCII.GetBytes(signatureBaseString)));
            }
        }

        private static void GetRequestParameters(Uri requestUri, ref Dictionary<string, string> queryString)
        {
            GetRequestParameters(requestUri.Query, ref queryString);
        }

        private static void GetRequestParameters(string parameters, ref Dictionary<string, string> queryString)
        {
            var splitParams = parameters.Replace("?", string.Empty).Split('&');
            foreach (var param in splitParams)
            {
                var keyValue = param.Split('=');

                queryString.Add(keyValue[0], keyValue[1]);
            }
        }
    }
}
