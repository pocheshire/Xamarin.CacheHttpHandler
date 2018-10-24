using System.Collections.Concurrent;
using System.Net.Http;
using CacheHandlerPlugin.Models;

namespace CacheHandlerPlugin.Services.CacheSettings
{
    public class SimpleCacheSettingsContainer : ICacheSettingsContainer
    {
        private readonly ConcurrentDictionary<string, RequestCacheSettings> _settings;

        public SimpleCacheSettingsContainer()
        {
            _settings = new ConcurrentDictionary<string, RequestCacheSettings>();
        }

        private string GetKey(HttpRequestMessage request)
        {
            return request.RequestUri.ToString();
        }

        public void Register(HttpRequestMessage request, RequestCacheSettings settings)
        {
            _settings.AddOrUpdate(GetKey(request), settings, (key, value) => settings);
        }

        public RequestCacheSettings Resolve(HttpRequestMessage request)
        {
            return _settings.TryGetValue(GetKey(request), out var value) 
                            ? value
                            : null;
        }
    }
}
