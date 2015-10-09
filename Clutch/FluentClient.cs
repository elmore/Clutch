using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Clutch
{
    public class FluentClient
    {
        private readonly string _rootUrl;

        public FluentClient(string rootUrl)
        {
            _rootUrl = rootUrl;
        }

        public async Task<T> Get<T>()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_rootUrl);

                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("rooms/1");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<T>();
                }
               
                return default(T) ;
            }
        }
    }
}
