using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Clutch
{
    /// Provides a configurationless (convention based) client for rest APIs
    /// 
    /// Given a resource at:
    ///     
    ///     http://my.api.com/v1/users/1/rooms/123
    /// 
    /// we can retrieve a populated model with:
    /// 
    ///     var room = new FluentClient("http://my.api.com/v1/").Find<User>(1).Get<Room>("123").Result;

    


    /// <summary>
    /// entry point. implements the interface but is just proxying to the CurriedRequest
    /// and handling persistent state
    /// </summary>
    public class FluentClient<TError> : IFluentRequest<TError>
    {
        private readonly HttpClientWrapper _client;

        public FluentClient(string rootUrl)
        {
            _client = new HttpClientWrapper(rootUrl);
        }

        public IFluentRequest<TError> Find<T>(object id)
        {
            return new CurriedRequest(_client).Find<T>(id);
        }

        public async Task<FluentResponse<T, TError>> Get<T>(object id)
        {
            return await new CurriedRequest(_client).Get<T>(id);
        }

        public async Task<FluentResponse<T, TError>> Post<T>(T model)
        {
            return await new CurriedRequest(_client).Post<T>(model);
        }

        /// <summary>
        /// wraps the .net HttpClient to remove some boilerplate code. attempts to 
        /// match the api of the HttpClient class
        /// </summary>
        internal class HttpClientWrapper
        {
            private readonly string _rootUrl;

            internal HttpClientWrapper(string rootUrl)
            {
                _rootUrl = rootUrl;
            }

            public async Task<FluentResponse<T, TError>> PostAsJsonAsync<T>(string url, object model)
            {
                using (HttpResponseMessage response = await CallRemote(c => c.PostAsJsonAsync(url, model)))
                {
                    return await BuildResponse<T>(response);
                }
            }

            public async Task<FluentResponse<T, TError>> GetAsync<T>(string url)
            {
                using (HttpResponseMessage response = await CallRemote(c => c.GetAsync(url)))
                {
                    return await BuildResponse<T>(response);
                }
            }

            private async Task<HttpResponseMessage> CallRemote(Func<HttpClient, Task<HttpResponseMessage>> httpAction)
            {
                using (var client = BuildClient())
                {
                    return await httpAction(client);
                }
            }

            private async Task<FluentResponse<T, TError>> BuildResponse<T>(HttpResponseMessage response)
            {
                var result = new FluentResponse<T, TError>();

                if (response.IsSuccessStatusCode)
                {
                    result.Entity = await response.Content.ReadAsAsync<T>();
                }
                else
                {
                    
                }

                // add in message

                result.StatusCode = response.StatusCode;

                return result;
            }

            private HttpClient BuildClient()
            {
                var client = new HttpClient();

                client.BaseAddress = new Uri(_rootUrl);

                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                return client;
            }
        }

        /// <summary>
        /// component that can chain together to build up requests to nested resources
        /// </summary>
        internal class CurriedRequest : IFluentRequest<TError>
        {
            private readonly HttpClientWrapper _client;
            private readonly Entity _path;

            internal CurriedRequest(HttpClientWrapper client)
            {
                _client = client;
                _path = new Entity();
            }

            public IFluentRequest<TError> Find<T>(object id)
            {
                _path.Chain(new PluralEntity<T>()).Chain(id);

                return this;
            }

            public async Task<FluentResponse<T, TError>> Get<T>(object id)
            {
                _path.Chain(new PluralEntity<T>()).Chain(id);

                return await _client.GetAsync<T>(_path.ToString());
            }

            public async Task<FluentResponse<T, TError>> Post<T>(T model)
            {
                _path.Chain(new PluralEntity<T>());

                return await _client.PostAsJsonAsync<T>(_path.ToString(), model);
            }
        }

        /// <summary>
        /// represents a url fragment. chains together nicely to create well formed paths
        /// </summary>
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

        /// <summary>
        /// responsible for intuiting the name of the class and pluralising it
        /// </summary>
        private class PluralEntity<T> : Entity
        {
            public PluralEntity(string postfix = "s")
                : base(string.Format("{0}{1}", typeof(T).Name, postfix))
            { }
        }
    }
}
