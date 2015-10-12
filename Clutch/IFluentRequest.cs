using System.Threading.Tasks;

namespace Clutch
{
    public interface IFluentRequest
    {
        IFluentRequest Find<T>(object id);
        Task<T> Get<T>(object id);
        Task<T> Post<T>(T model);
    }
}