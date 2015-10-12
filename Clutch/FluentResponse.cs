using System.Net;

namespace Clutch
{
    public class FluentResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public T Entity { get; set; }
    }
}