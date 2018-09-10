using System.Net.Http;
using System.Threading.Tasks;
using CacheHandlerPlugin.Services.Connectivity;
using Plugin.Connectivity.Abstractions;

namespace CacheHandlerPlugin.Connectivity
{
    public class ConnectivityService : IConnectivityService
    {
        private IConnectivity Connectivity { get; }

        public ConnectivityService(IConnectivity connectivity = null)
        {
            Connectivity = connectivity ?? Plugin.Connectivity.CrossConnectivity.Current;
        }

        public Task<bool> CanDoRequest(HttpRequestMessage request)
        {
            return Task.FromResult(Connectivity.IsConnected);
        }
    }
}
