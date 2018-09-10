using System.Net.Http;
using CacheHandlerPlugin.Models;

namespace CacheHandlerPlugin.Services.CacheSettings
{
    public interface ICacheSettingsContainer
    {
        RequestCacheSettings Resolve(HttpRequestMessage request);
    }
}
