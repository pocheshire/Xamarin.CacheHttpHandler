using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System;
using CacheHandlerPlugin.Models;
using CacheHandlerPlugin.UnitTest.Mocks;

namespace CacheHandlerPlugin.UnitTest
{
    [Collection("UnitTest")]
    public class CacheHandlerTests
    {
        private readonly Uri _requestUri = new Uri("http://google.com");

        private readonly UnitTestFixture _fixture;

        public CacheHandlerTests(UnitTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CheckDataLoadedWithoutCache()
        {
            _fixture.Reset();

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, _requestUri))
            {
                var responseMessage = await _fixture.HttpClient.SendAsync(requestMessage);

                Assert.False(_fixture.ApiCacheService.TryGet(requestMessage, out responseMessage));
            }
        }

        [Fact]
        public async Task CheckDataSavedInCache_RequestProperties()
        {
            _fixture.Reset();

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, _requestUri))
            {
                requestMessage.Properties.Add(CacheMessageHandler.CachedKey, new RequestCacheSettings(600));

                var responseMessage = await _fixture.HttpClient.SendAsync(requestMessage);

                Assert.True(_fixture.ApiCacheService.TryGet(requestMessage, out responseMessage));
            }
        }

        [Fact]
        public async Task CheckDataSavedInCache_CSC()
        {
            var container = new CacheSettingsContainerMock();

            _fixture.SetCacheSettingsContainer(container);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, _requestUri))
            {
                container.Register(requestMessage, new RequestCacheSettings(600));

                var responseMessage = await _fixture.HttpClient.SendAsync(requestMessage);

                Assert.True(_fixture.ApiCacheService.TryGet(requestMessage, out responseMessage));
            }
        }
    }
}