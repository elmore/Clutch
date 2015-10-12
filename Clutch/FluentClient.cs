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
    public class FluentClient
    {
        private readonly string _rootUrl;
        private readonly Entity _path = new Entity();

        public FluentClient(string rootUrl)
        {
            _rootUrl = rootUrl;
        }

        public async Task<T> Get<T>(object id)
        {
            _path.Chain(new PluralEntity<T>()).Chain(id);

            return await new HttpClientWrapper(_rootUrl).GetAsync<T>(_path.ToString());
        }

        public FluentClient Find<T>(object id)
        {
            _path.Chain(new PluralEntity<T>()).Chain(id);

            return this;
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
