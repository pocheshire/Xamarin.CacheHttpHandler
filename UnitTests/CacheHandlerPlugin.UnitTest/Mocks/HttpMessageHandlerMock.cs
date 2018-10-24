using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CacheHandlerPlugin.UnitTest.Mocks
{
    public class HttpMessageHandlerMock : HttpMessageHandler
    {
        private string _json = string.Empty;

        public bool IsBadRequest { get; set; }

        public bool IsBadRequestWithException { get; set; }

        public HttpStatusCode BadRequestCode => HttpStatusCode.ServiceUnavailable;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_json))
            {
                //TODO: load from content.json
            }

            return IsBadRequest 
                ? (IsBadRequestWithException 
                    ? throw new HttpRequestException("Some reason of bad request")
                    : Task.FromResult(new HttpResponseMessage(BadRequestCode)
                    {
                        Content = new StringContent(_json)
                    }))
                : Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_json)
                });
        }
    }
}