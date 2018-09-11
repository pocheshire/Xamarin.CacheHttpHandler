using System;
using System.Net.Http;
using CacheHandlerPlugin.Realm;
using CacheHandlerPlugin.Realm.Services.Repository;
using CacheHandlerPlugin.Services.ApiCache;
using CacheHandlerPlugin.Services.CacheSettings;
using CacheHandlerPlugin.Services.Connectivity;
using CacheHandlerPlugin.UnitTest.Mocks;
using Xunit;

namespace CacheHandlerPlugin.UnitTest
{
    [CollectionDefinition("UnitTest")]
    public class TestCollection : ICollectionFixture<UnitTestFixture>
    {

    }

    public class UnitTestFixture : IDisposable
    {
        public HttpClient HttpClient { get; private set; }      

        public CacheMessageHandler MessageHandler { get; private set; }

        public ICacheSettingsContainer CacheSettingsContainer { get; private set; }

        public IApiCacheService ApiCacheService { get; private set; }

        public IConnectivityService ConnectivityService { get; private set; }

        public UnitTestFixture()
        {
            Setup();
        }

        ~UnitTestFixture()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Reset();
            }
        }

        public void Reset()
        {
            var repository = new RealmRepository(null);
            repository.RemoveAll();

            ApiCacheService = new RealmApiCacheService(repository);

            ConnectivityService = new ConnectivityMock();

            var mockHttpMessageHandler = new HttpMessageHandlerMock();

            MessageHandler = new CacheMessageHandler(mockHttpMessageHandler, ApiCacheService, ConnectivityService)
            {
                CacheSettingsContainer = CacheSettingsContainer
            };

            HttpClient = new HttpClient(MessageHandler);
        }

        public void Setup()
        {
            Reset();
        }

        public void SetCacheSettingsContainer(ICacheSettingsContainer container)
        {
            CacheSettingsContainer = container;

            Reset();
        }
    }
}
