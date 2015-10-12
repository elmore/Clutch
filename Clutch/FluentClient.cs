using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Clutch
{
    /// <summary>
    /// Provides a configurationless (convention based) client for rest APIs
    /// 
    /// =USAGE=
    /// 
    /// given a resource at 
    ///     
    ///     http://my.api.com/v1/users/1/rooms/123
    /// 
    /// we can retrieve a populated model with:
    /// 
    ///     var room = new FluentClient("http://my.api.com/v1/").Find<User>(1).Get<Room>("123").Result;
    /// 
    /// </summary>

    public interface IFluentRequest
    {
        IFluentRequest Find<T>(object id);
        Task<T> Get<T>(object id);
        Task<bool> Post<T>(object model);
    }

    public class FluentClient : IFluentRequest
    {
        private readonly HttpClientWrapper _client;

        public FluentClient(string rootUrl)
        {
            _client = new HttpClientWrapper(rootUrl);
        }

        public IFluentRequest Find<T>(object id)
        {
            return new CurriedRequest(_client).Find<T>(id);
        }

        public async Task<T> Get<T>(object id)
        {
            return await new CurriedRequest(_client).Get<T>(id);
        }

        public async Task<bool> Post<T>(object model)
        {
            return await new CurriedRequest(_client).Post<T>(model);
        }



        internal class HttpClientWrapper
        {
            private readonly string _rootUrl;

            internal HttpClientWrapper(string rootUrl)
            {
                _rootUrl = rootUrl;
            }

            public async Task<bool> PostAsJsonAsync(string url, object model)
            {
                using (var client = GetClient())
                using (HttpResponseMessage response = await client.PostAsJsonAsync(url, model))
                {
                    return response.IsSuccessStatusCode;
                }
            }

            public async Task<T> GetAsync<T>(string url)
            {
                using (var client = GetClient())
                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsAsync<T>();
                    }

                    return default(T);
                }
            }

            private HttpClient GetClient()
            {
                var client = new HttpClient();

                client.BaseAddress = new Uri(_rootUrl);

                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                return client;
            }
        }



        public class CurriedRequest : IFluentRequest
        {
            private readonly HttpClientWrapper _client;
            private readonly Entity _path;

            internal CurriedRequest(HttpClientWrapper client)
            {
                _client = client;
                _path = new Entity();
            }

            public IFluentRequest Find<T>(object id)
            {
                _path.Chain(new PluralEntity<T>()).Chain(id);

                return this;
            }

            public async Task<T> Get<T>(object id)
            {
                _path.Chain(new PluralEntity<T>()).Chain(id);

                return await _client.GetAsync<T>(_path.ToString());
            }

            public async Task<bool> Post<T>(object model)
            {
                _path.Chain(new PluralEntity<T>());

                return await _client.PostAsJsonAsync(_path.ToString(), model);
            }
        }






        private class Entity
        {
            private Entity _chainLink = null;
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

            public Entity Chain(object link)
            {
                var wrapped = link as Entity ?? new Entity(link);

                if (_chainLink != null)
                {
                    _chainLink.Chain(wrapped);
                }
                else
                {
                    _chainLink = wrapped;
                }

                return this;
            }
        }

        private class PluralEntity<T> : Entity
        {
            public PluralEntity(string postfix = "s")
                : base(string.Format("{0}{1}", typeof(T).Name, postfix))
            { }
        }





    }
}
