using System.Net.Http;
using System.Threading.Tasks;

namespace CacheHandlerPlugin.Services.Connectivity
{
    public interface IConnectivityService
    {
        Task<bool> CanDoRequest(HttpRequestMessage request);
    }
}
