using System.Threading.Tasks;

namespace Clutch
{
    public interface IFluentRequest<TError>
    {
        IFluentRequest<TError> Find<T>(object id);
        Task<FluentResponse<T, TError>> Get<T>(object id);
        Task<FluentResponse<T, TError>> Post<T>(T model);
    }
}