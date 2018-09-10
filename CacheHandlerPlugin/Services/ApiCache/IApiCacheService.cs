using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CacheHandlerPlugin.Services.ApiCache
{
    public interface IApiCacheService
    {
        bool TryGet(HttpRequestMessage requestMessage, out HttpResponseMessage responseMessage);

        Task Add(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage, TimeSpan expireIn);
    }
}
