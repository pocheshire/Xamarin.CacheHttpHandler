using System;
using System.Net.Http;

namespace CacheHandlerPlugin.Services.ApiCache
{
    public interface IApiCacheService
    {
        bool TryGet(HttpRequestMessage requestMessage, out HttpResponseMessage responseMessage);

        void Add(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage, TimeSpan expireIn);
    }
}
