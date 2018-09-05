using System.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace TwitterApi
{
    public static class HttpContentExtensions
    {
        public static async Task<JsonValue> ReadAsJsonValueAsync(this HttpContent httpContent)
        {
            var data = await httpContent.ReadAsStringAsync();
            return JsonValue.Parse(data);
        }
    }
}
