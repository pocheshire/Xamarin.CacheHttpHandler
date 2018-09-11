using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CacheHandlerPlugin.UnitTest.Mocks
{
    public class HttpMessageHandlerMock : HttpMessageHandler
    {
        private string _json = string.Empty;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_json))
            {
                //TODO: load from content.json
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_json)
            });
        }
    }
}