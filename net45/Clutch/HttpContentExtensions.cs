using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Clutch
{
    static class HttpContentExtensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            string json = await content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
