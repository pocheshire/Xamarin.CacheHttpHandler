using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CacheHandlerPlugin.Models;
using CacheHandlerPlugin.Services.ApiCache;
using CacheHandlerPlugin.Exceptions;
using CacheHandlerPlugin.Services.Connectivity;

namespace CacheHandlerPlugin
{
    public class CacheMessageHandler : HttpMessageHandler
    {
        public const string CACHED_KEY = "X-CacheHandler-Key";

        private readonly HttpMessageInvoker _httpMessageInvoker;

        IApiCacheService ApiCacheService { get; }

        IConnectivityService ConnectivityService { get; }

        public CacheMessageHandler(IApiCacheService apiCache, IConnectivityService connectivity, HttpMessageHandler httpMessageHandler)
        {
            _httpMessageInvoker = new HttpMessageInvoker(httpMessageHandler);

            ApiCacheService = apiCache;
            ConnectivityService = connectivity;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;

            RequestCacheSettings cacheProperty = null;

            if (request.Properties.ContainsKey(CACHED_KEY))
                cacheProperty = request.Properties[CACHED_KEY] as RequestCacheSettings;

            if (cacheProperty != null && cacheProperty.IsCacheable)
            {
                var cacheService = ApiCacheService;
                switch (cacheProperty.Policy)
                {
                    case CachePolicy.CacheFirst:
                        response = await GetFromCacheOrSendRequest(request, cacheProperty.ExpireInSeconds, cacheService, cancellationToken);
                        break;
                    case CachePolicy.RequestFirst:
                        response = await SendRequestOrGetFromCache(request, cacheProperty.ExpireInSeconds, cacheService, cancellationToken);
                        break;
                }
            }
            else
            {
                response = await _httpMessageInvoker.SendAsync(request, cancellationToken);
            }

            return response;
        }

        private async Task<HttpResponseMessage> SendRequestOrGetFromCache(HttpRequestMessage request, int expireInSeconds, IApiCacheService cacheService, CancellationToken cancellationToken)
        {
            var responseMessage = new HttpResponseMessage();

            Exception exception = null;

            var canDoRequest = await ConnectivityService.CanDoRequest(request);
            if (canDoRequest)
            {
                try
                {
                    responseMessage = await _httpMessageInvoker.SendAsync(request, cancellationToken);
                }
                catch (Exception ex)
                {
                    responseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    exception = ex;
                }
            }
            else
            {
                responseMessage.StatusCode = System.Net.HttpStatusCode.BadGateway;
                exception = new ConnectivityException(request);
            }

            if (responseMessage.IsSuccessStatusCode)
            {
                cacheService.Add(request, responseMessage, TimeSpan.FromSeconds(expireInSeconds));
            }
            else if (!cacheService.TryGet(request, out responseMessage) && exception != null)
            {
                throw exception;
            }

            return responseMessage;
        }

        private async Task<HttpResponseMessage> GetFromCacheOrSendRequest(HttpRequestMessage request, int expireInSeconds, IApiCacheService cacheService, CancellationToken cancellationToken)
        {
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                RequestMessage = request
            };

            if (!cacheService.TryGet(request, out responseMessage))
            {
                responseMessage = await _httpMessageInvoker.SendAsync(request, cancellationToken);

                if (responseMessage.IsSuccessStatusCode)
                {
                    cacheService.Add(request, responseMessage, TimeSpan.FromSeconds(expireInSeconds));
                }
            }

            return responseMessage;
        }
    }
}
