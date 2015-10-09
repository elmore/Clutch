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

        public async Task<T> Get<T>(object id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_rootUrl);

                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string url = string.Format("{0}s/{1}", typeof(T).Name, id);

                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<T>();
                }
               
                return default(T) ;
            }
        }
    }
}
