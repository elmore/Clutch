using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Clutch
{
    static class HttpContentExtensions
    {
        public static Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            return content.ReadAsStringAsync()
                .ContinueWith(t => JsonConvert.DeserializeObject<T>(t.Result));
        }
    }
}
