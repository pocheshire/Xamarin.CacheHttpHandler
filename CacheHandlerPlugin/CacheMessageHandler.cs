using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CacheHandlerPlugin.Exceptions;
using CacheHandlerPlugin.Models;
using CacheHandlerPlugin.Services.ApiCache;
using CacheHandlerPlugin.Services.CacheSettings;
using CacheHandlerPlugin.Services.Connectivity;

namespace CacheHandlerPlugin
{
    public class CacheMessageHandler : HttpMessageHandler
    {
        public const string CachedKey = "X-CacheHandler-Key";

        private readonly HttpMessageInvoker _httpMessageInvoker;

        private IApiCacheService ApiCacheService { get; }

        private IConnectivityService ConnectivityService { get; }

        public ICacheSettingsContainer CacheSettingsContainer { get; set; }

        public CacheMessageHandler(HttpMessageHandler httpMessageHandler, IApiCacheService apiCache, IConnectivityService connectivity)
        {
            _httpMessageInvoker = new HttpMessageInvoker(httpMessageHandler);

            ApiCacheService = apiCache;
            ConnectivityService = connectivity;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;

            RequestCacheSettings cacheProperty = null;

            if (CacheSettingsContainer != null)
                cacheProperty = CacheSettingsContainer.Resolve(request);
            else if (request.Properties.ContainsKey(CachedKey))
                cacheProperty = request.Properties[CachedKey] as RequestCacheSettings;

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
                await cacheService.Add(request, responseMessage, TimeSpan.FromSeconds(expireInSeconds));
            }
            else if (!cacheService.TryGet(request, out responseMessage) && exception != null)
            {
                throw exception;
            }

            return responseMessage;
        }

		private async Task<HttpResponseMessage> GetFromCacheOrSendRequest(HttpRequestMessage request, int expireInSeconds, IApiCacheService cacheService, CancellationToken cancellationToken)
        {
            if (cacheService.TryGet(request, out var responseMessage)) 
                return responseMessage;
            
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
                await cacheService.Add(request, responseMessage, TimeSpan.FromSeconds(expireInSeconds));
            }
            else if (exception != null)
            {
                throw exception;
            }

            return responseMessage;
        }
    }
}
