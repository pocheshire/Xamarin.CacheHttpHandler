using System.Net.Http;
using System.Threading.Tasks;
using CacheHandlerPlugin.Services.Connectivity;

namespace CacheHandlerPlugin.UnitTest.Mocks
{
    public class ConnectivityMock : IConnectivityService
    {
        public bool IsConnected { get; set; } = true;

        public Task<bool> CanDoRequest(HttpRequestMessage request)
        {
            return Task.FromResult(IsConnected);
        }
    }
}