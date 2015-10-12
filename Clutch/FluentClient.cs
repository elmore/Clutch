using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Clutch
{
    public class FluentClient
    {
        private readonly string _rootUrl;
        private Entity _path = new Entity();

        public FluentClient(string rootUrl)
        {
            _rootUrl = rootUrl;
        }

        public async Task<T> Get<T>(object id)
        {
            _path.Chain(new PluralEntity<T>()).Chain(new Entity(id));

            //new PluralEntity<T>().Chain(new Entity(id)).Chain(_path);

            return await new HttpClientWrapper(_rootUrl).GetAsync<T>(_path.ToString());
        }

        public FluentClient Find<T>(object id)
        {
            _path.Chain(new PluralEntity<T>()).Chain(new Entity(id));

            return this;
        }

        private class Entity
        {
            private object _chainLink = null;
            private readonly object _value = null;

            public Entity(object val = null)
            {
                _value = val;
            }

            public override string ToString()
            {
                if (_chainLink == null)
                {
                    return _value.ToString();
                }

                if (_value == null)
                {
                    return _chainLink.ToString();
                }

                return string.Format("{0}/{1}", _value, _chainLink);
            }

            public Entity Chain(Entity link)
            {
                _chainLink = link;

                return link;
            }
        }

        private class PluralEntity<T> : Entity
        {
            public PluralEntity(string postfix = "s")
                : base(string.Format("{0}{1}", typeof(T).Name, postfix))
            { }
        }

        private class HttpClientWrapper
        {
            private readonly string _rootUrl;

            public HttpClientWrapper(string rootUrl)
            {
                _rootUrl = rootUrl;
            }

            public async Task<T> GetAsync<T>(string url)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_rootUrl);

                    client.DefaultRequestHeaders.Accept.Clear();

                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsAsync<T>();
                    }

                    return default(T);
                }
            }
        }
    }
}
