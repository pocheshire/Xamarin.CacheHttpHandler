using System.Net.Http;
using CacheHandlerPlugin.Models;

namespace CacheHandlerPlugin.Services.CacheSettings
{
    public interface ICacheSettingsContainer
    {
        void Register(HttpRequestMessage request, RequestCacheSettings settings);

        RequestCacheSettings Resolve(HttpRequestMessage request);
    }
}
