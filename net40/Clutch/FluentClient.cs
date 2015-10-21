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
    public class FluentClient<TError> : IFluentRequest<TError> where TError : new()
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

        public Task<FluentResponse<T, TError>> Get<T>(object id)
        {
            return new CurriedRequest(_client).Get<T>(id);
        }

        public Task<FluentResponse<T, TError>> Post<T>(T model)
        {
            return new CurriedRequest(_client).Post<T>(model);
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

            public Task<FluentResponse<T, TError>> PostAsJsonAsync<T>(string url, object model)
            {
                var client = BuildClient();

                var response = client.PostAsJsonAsync(url, model);

                return BuildResponse<T>(response.Result);
            }

            public Task<FluentResponse<T, TError>> GetAsync<T>(string url)
            {
                var client = BuildClient();

                var response = client.GetAsync(url);

                return BuildResponse<T>(response.Result);
            }

            private Task<FluentResponse<T, TError>> BuildResponse<T>(HttpResponseMessage response)
            {
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsAsync<T>().ContinueWith(t =>
                    {
                        var result = new FluentResponse<T, TError>();

                        result.Entity = t.Result;

                        result.StatusCode = response.StatusCode;

                        return result;
                    });
                }

                return response.Content.ReadAsAsync<TError>().ContinueWith(t =>
                {
                    var result = new FluentResponse<T, TError>();

                    result.Error = t.Result;

                    result.StatusCode = response.StatusCode;

                    return result;
                });
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

            public Task<FluentResponse<T, TError>> Get<T>(object id)
            {
                _path.Chain(new PluralEntity<T>()).Chain(id);

                return _client.GetAsync<T>(_path.ToString());
            }

            public Task<FluentResponse<T, TError>> Post<T>(T model)
            {
                _path.Chain(new PluralEntity<T>());

                return _client.PostAsJsonAsync<T>(_path.ToString(), model);
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
