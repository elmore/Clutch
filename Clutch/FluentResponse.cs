using System.Net;

namespace Clutch
{
    public class FluentResponse<TSuccess, TError>
    {
        public HttpStatusCode StatusCode { get; set; }
        public TError Error { get; set; }
        public TSuccess Entity { get; set; }
    }
}