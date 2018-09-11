using System;
using System.Collections.Generic;
using System.Net.Http;
using CacheHandlerPlugin.Models;
using CacheHandlerPlugin.Services.CacheSettings;

namespace CacheHandlerPlugin.UnitTest.Mocks
{
    public class CacheSettingsContainerMock : ICacheSettingsContainer
    {
        private readonly Dictionary<string, RequestCacheSettings> _settings;

        public CacheSettingsContainerMock()
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
