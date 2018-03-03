using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TwitterApi
{
    public class TwitterClient : IDisposable
    {
        private readonly string _consumerKey;
        private readonly string _consumerKeySecret;
        private readonly string _accessToken;
        private readonly string _accessTokenSecret;
        private readonly HttpClient _httpClient;

        public TwitterClient(string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret)
        {
            _consumerKey = consumerKey;
            _consumerKeySecret = consumerKeySecret;
            _accessToken = accessToken;
            _accessTokenSecret = accessTokenSecret;
            _httpClient = new HttpClient();
        }

        public async Task<string> CreateCollection(string name, string description)
        {
            const string Url = "https://api.twitter.com/1.1/collections/create.json";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Url)
            {
                Content = new StringContent(
                    $"name={name.ToEscapedString()}&description={description.ToEscapedString()}", 
                    Encoding.UTF8, 
                    "application/x-www-form-urlencoded")
            };

            httpRequestMessage.SetTwitterOAuthHeader(_consumerKey, _consumerKeySecret, _accessToken, _accessTokenSecret);
            var response = await _httpClient.SendAsync(httpRequestMessage);

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> AddCollectionEntry(long collectionId, long tweetId)
        {
            var url = $"https://api.twitter.com/1.1/collections/entries/add.json?tweet_id={tweetId}&id=custom-{collectionId}";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequestMessage.SetTwitterOAuthHeader(_consumerKey, _consumerKeySecret, _accessToken, _accessTokenSecret);
            var response = await _httpClient.SendAsync(httpRequestMessage);

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetTweetById(long id)
        {
            var url = $"https://api.twitter.com/1.1/statuses/show.json?id={id}";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequestMessage.SetTwitterOAuthHeader(_consumerKey, _consumerKeySecret, _accessToken, _accessTokenSecret);
            var response =  await _httpClient.SendAsync(httpRequestMessage);

            return await response.Content.ReadAsStringAsync();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
