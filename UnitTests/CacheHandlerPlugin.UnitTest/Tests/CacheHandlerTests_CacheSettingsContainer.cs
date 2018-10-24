using System;
using System.Net.Http;
using System.Threading.Tasks;
using CacheHandlerPlugin.Exceptions;
using CacheHandlerPlugin.Models;
using CacheHandlerPlugin.Services.CacheSettings;
using CacheHandlerPlugin.UnitTest.Mocks;
using Xunit;

namespace CacheHandlerPlugin.UnitTest.Tests
{
    [Collection("UnitTest")]
    public class CacheHandlerTests_CacheSettingsContainer : BaseTests
    {
        public CacheHandlerTests_CacheSettingsContainer(UnitTestFixture fixture)
            : base(fixture) => Fixture.Reset();

        [Fact]
        public async Task CheckDataSavedInCache_CSC()
        {
            var container = new SimpleCacheSettingsContainer();

            Fixture.SetCacheSettingsContainer(container);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                container.Register(requestMessage, new RequestCacheSettings(600));

                var responseMessage = await Fixture.HttpClient.SendAsync(requestMessage);

                Assert.True(Fixture.ApiCacheService.TryGet(requestMessage, out responseMessage));
            }
        }

        [Fact]
        public async Task CheckDataSavedInCache_CSC_WithLostConnectionOnRefresh()
        {
            string cachedContent;

            var container = new SimpleCacheSettingsContainer();

            Fixture.SetCacheSettingsContainer(container);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                container.Register(requestMessage, new RequestCacheSettings(600));

                var responseMessage = await Fixture.HttpClient.SendAsync(requestMessage);

                var responseContent = await responseMessage.Content.ReadAsStringAsync();

                Assert.True(Fixture.ApiCacheService.TryGet(requestMessage, out var cacheResponseMessage));

                cachedContent = await cacheResponseMessage.Content.ReadAsStringAsync();

                Assert.Equal(responseContent, cachedContent);
            }

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                (Fixture.ConnectivityService as ConnectivityMock).IsConnected = false;

                container.Register(requestMessage, new RequestCacheSettings(600));

                var responseMessage = await Fixture.HttpClient.SendAsync(requestMessage);

                var responseContent = await responseMessage.Content.ReadAsStringAsync();

                Assert.Equal(responseContent, cachedContent);

                (Fixture.ConnectivityService as ConnectivityMock).IsConnected = true;
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CheckDataSavedInCache_CSC_WithBadRequestOnRefresh(bool withException)
        {
            string cachedContent;

            var container = new SimpleCacheSettingsContainer();

            Fixture.SetCacheSettingsContainer(container);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                container.Register(requestMessage, new RequestCacheSettings(600));

                var responseMessage = await Fixture.HttpClient.SendAsync(requestMessage);

                var responseContent = await responseMessage.Content.ReadAsStringAsync();

                Assert.True(Fixture.ApiCacheService.TryGet(requestMessage, out var cacheResponseMessage));

                cachedContent = await cacheResponseMessage.Content.ReadAsStringAsync();

                Assert.Equal(responseContent, cachedContent);
            }

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                Fixture.MockHttpMessageHandler.IsBadRequest = true;
                Fixture.MockHttpMessageHandler.IsBadRequestWithException = withException;

                container.Register(requestMessage, new RequestCacheSettings(600));

                var responseMessage = await Fixture.HttpClient.SendAsync(requestMessage);

                var responseContent = await responseMessage.Content.ReadAsStringAsync();

                Assert.Equal(responseContent, cachedContent);

                Fixture.MockHttpMessageHandler.IsBadRequest = false;
            }
        }

        [Fact]
        public async Task CheckDataSavedInCache_CSC_NoInternet()
        {
            var container = new SimpleCacheSettingsContainer();

            Fixture.SetCacheSettingsContainer(container);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                (Fixture.ConnectivityService as ConnectivityMock).IsConnected = false;

                container.Register(requestMessage, new RequestCacheSettings(600));

                await Assert.ThrowsAsync<ConnectivityException>(async () => await Fixture.HttpClient.SendAsync(requestMessage));

                (Fixture.ConnectivityService as ConnectivityMock).IsConnected = true;
            }
        }

        [Fact]
        public async Task CheckDataSavedInCache_CSC_WithException()
        {
            var container = new SimpleCacheSettingsContainer();

            Fixture.SetCacheSettingsContainer(container);
            Fixture.MockHttpMessageHandler.IsBadRequest = true;
            Fixture.MockHttpMessageHandler.IsBadRequestWithException = true;

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                container.Register(requestMessage, new RequestCacheSettings(600));

                await Assert.ThrowsAnyAsync<Exception>(async () => await Fixture.HttpClient.SendAsync(requestMessage));
            }

            Fixture.MockHttpMessageHandler.IsBadRequest = false;
            Fixture.MockHttpMessageHandler.IsBadRequestWithException = false;
        }

        [Fact]
        public async Task CheckDataSavedInCache_CSC_WithBadRequest()
        {
            var container = new SimpleCacheSettingsContainer();

            Fixture.SetCacheSettingsContainer(container);
            Fixture.MockHttpMessageHandler.IsBadRequest = true;

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                container.Register(requestMessage, new RequestCacheSettings(600));

                var responseMessage = await Fixture.HttpClient.SendAsync(requestMessage);

                Assert.True(responseMessage.StatusCode == Fixture.MockHttpMessageHandler.BadRequestCode);
            }

            Fixture.MockHttpMessageHandler.IsBadRequest = false;
        }
    }
}
