using System.Json;
using System.Threading.Tasks;

namespace TwitterApi
{
    public interface ITwitterClient
    {
        Task<JsonValue> CreateCollection(string name, string description);

        Task<JsonValue> AddCollectionEntry(long collectionId, long tweetId);

        Task<JsonValue> GetTweetById(long id);
    }
}
