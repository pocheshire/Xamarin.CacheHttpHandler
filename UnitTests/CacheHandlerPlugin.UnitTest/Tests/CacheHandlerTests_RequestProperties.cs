using System;
using System.Net.Http;
using System.Threading.Tasks;
using CacheHandlerPlugin.Exceptions;
using CacheHandlerPlugin.Models;
using CacheHandlerPlugin.UnitTest.Mocks;
using Xunit;

namespace CacheHandlerPlugin.UnitTest.Tests
{
    [Collection("UnitTest")]
    public class CacheHandlerTests_RequestProperties : BaseTests
    {
        public CacheHandlerTests_RequestProperties(UnitTestFixture fixture)
            : base(fixture) => Fixture.Reset();

        [Fact]
        public async Task CheckDataSavedInCache_RequestProperties()
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                requestMessage.Properties.Add(CacheMessageHandler.CachedKey, new RequestCacheSettings(600));

                var responseMessage = await Fixture.HttpClient.SendAsync(requestMessage);

                Assert.True(Fixture.ApiCacheService.TryGet(requestMessage, out responseMessage));
            }
        }

        [Fact]
        public async Task CheckDataSavedInCache_RequestProperties_WithLostConnectionOnRefresh()
        {
            string cachedContent;

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                requestMessage.Properties.Add(CacheMessageHandler.CachedKey, new RequestCacheSettings(600));

                var responseMessage = await Fixture.HttpClient.SendAsync(requestMessage);

                var responseContent = await responseMessage.Content.ReadAsStringAsync();

                Assert.True(Fixture.ApiCacheService.TryGet(requestMessage, out var cacheResponseMessage));

                cachedContent = await cacheResponseMessage.Content.ReadAsStringAsync();

                Assert.Equal(responseContent, cachedContent);
            }

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                (Fixture.ConnectivityService as ConnectivityMock).IsConnected = false;

                requestMessage.Properties.Add(CacheMessageHandler.CachedKey, new RequestCacheSettings(600));

                var responseMessage = await Fixture.HttpClient.SendAsync(requestMessage);

                var responseContent = await responseMessage.Content.ReadAsStringAsync();

                Assert.Equal(responseContent, cachedContent);

                (Fixture.ConnectivityService as ConnectivityMock).IsConnected = true;
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CheckDataSavedInCache_RequestProperties_WithBadRequestOnRefresh(bool withException)
        {
            string cachedContent;

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                requestMessage.Properties.Add(CacheMessageHandler.CachedKey, new RequestCacheSettings(600));

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

                requestMessage.Properties.Add(CacheMessageHandler.CachedKey, new RequestCacheSettings(600));

                var responseMessage = await Fixture.HttpClient.SendAsync(requestMessage);

                var responseContent = await responseMessage.Content.ReadAsStringAsync();

                Assert.Equal(responseContent, cachedContent);

                Fixture.MockHttpMessageHandler.IsBadRequest = false;
            }
        }

        [Fact]
        public async Task CheckDataSavedInCache_RequestProperties_NoInternet()
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                (Fixture.ConnectivityService as ConnectivityMock).IsConnected = false;

                requestMessage.Properties.Add(CacheMessageHandler.CachedKey, new RequestCacheSettings(600));

                await Assert.ThrowsAsync<ConnectivityException>(async () => await Fixture.HttpClient.SendAsync(requestMessage));

                (Fixture.ConnectivityService as ConnectivityMock).IsConnected = true;
            }
        }

        [Fact]
        public async Task CheckDataSavedInCache_RequestProperties_WithException()
        {
            Fixture.MockHttpMessageHandler.IsBadRequest = true;
            Fixture.MockHttpMessageHandler.IsBadRequestWithException = true;

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                requestMessage.Properties.Add(CacheMessageHandler.CachedKey, new RequestCacheSettings(600));

                await Assert.ThrowsAnyAsync<Exception>(async () => await Fixture.HttpClient.SendAsync(requestMessage));
            }

            Fixture.MockHttpMessageHandler.IsBadRequest = false;
            Fixture.MockHttpMessageHandler.IsBadRequestWithException = false;
        }

        [Fact]
        public async Task CheckDataSavedInCache_RequestProperties_WithBadRequest()
        {
            Fixture.MockHttpMessageHandler.IsBadRequest = true;

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                requestMessage.Properties.Add(CacheMessageHandler.CachedKey, new RequestCacheSettings(600));

                var responseMessage = await Fixture.HttpClient.SendAsync(requestMessage);

                Assert.True(responseMessage.StatusCode == Fixture.MockHttpMessageHandler.BadRequestCode);
            }

            Fixture.MockHttpMessageHandler.IsBadRequest = false;
        }
    }
}
