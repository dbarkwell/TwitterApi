using System;
using System.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TwitterApi
{
    public class TwitterClient : ITwitterClient, IDisposable
    {
        private readonly string _consumerKey;
        private readonly string _consumerKeySecret;
        private readonly string _accessToken;
        private readonly string _accessTokenSecret;
        private readonly HttpClient _httpClient;

        private const string TwitterApiUrl = "https://api.twitter.com/1.1";

        public TwitterClient(string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret)
        {
            _consumerKey = consumerKey;
            _consumerKeySecret = consumerKeySecret;
            _accessToken = accessToken;
            _accessTokenSecret = accessTokenSecret;
            _httpClient = new HttpClient();
        }

        public async Task<JsonValue> CreateCollection(string name, string description)
        {
            var url = $"{TwitterApiUrl}/collections/create.json";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    $"name={name.ToEscapedString()}&description={description.ToEscapedString()}", 
                    Encoding.UTF8, 
                    "application/x-www-form-urlencoded")
            };

            httpRequestMessage.SetTwitterOAuthHeader(_consumerKey, _consumerKeySecret, _accessToken, _accessTokenSecret);
            var response = await _httpClient.SendAsync(httpRequestMessage);

            return await response.Content.ReadAsJsonValueAsync();
        }

        public async Task<JsonValue> AddCollectionEntry(long collectionId, long tweetId)
        {
            var url = $"{TwitterApiUrl}/collections/entries/add.json?tweet_id={tweetId}&id=custom-{collectionId}";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequestMessage.SetTwitterOAuthHeader(_consumerKey, _consumerKeySecret, _accessToken, _accessTokenSecret);
            var response = await _httpClient.SendAsync(httpRequestMessage);

            return await response.Content.ReadAsJsonValueAsync();
        }

        public async Task<JsonValue> GetTweetById(long id)
        {
            var url = $"{TwitterApiUrl}/statuses/show.json?id={id}";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequestMessage.SetTwitterOAuthHeader(_consumerKey, _consumerKeySecret, _accessToken, _accessTokenSecret);
            var response =  await _httpClient.SendAsync(httpRequestMessage);

            return await response.Content.ReadAsJsonValueAsync();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
