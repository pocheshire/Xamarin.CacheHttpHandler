using System;
using System.Net.Http;
using CacheHandlerPlugin.Models;
using CacheHandlerPlugin.Services.CacheSettings;
using System.Collections.Generic;

namespace Playground.Core.Cache
{
    public class CacheSettingsContainer : ICacheSettingsContainer
    {
        private readonly Dictionary<string, RequestCacheSettings> _settings;

        public CacheSettingsContainer()
        {
            _settings = new Dictionary<string, RequestCacheSettings>();
        }

        private string GetKey(HttpRequestMessage request)
        {
            return request.RequestUri.ToString();
        }

        internal void Register(HttpRequestMessage request, RequestCacheSettings settings)
        {
            _settings[GetKey(request)] = settings;
        }

        public RequestCacheSettings Resolve(HttpRequestMessage request)
        {
            return _settings.TryGetValue(GetKey(request), out var value) ?
                            value
                                :
                            null;
        }
    }
}
