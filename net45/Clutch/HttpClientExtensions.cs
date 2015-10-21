using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Clutch
{
    static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PostAsJsonAsync(this HttpClient b, string url, object model)
        {
            string json = JsonConvert.SerializeObject(model);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return b.PostAsync(url, content);
        }
    }
}
