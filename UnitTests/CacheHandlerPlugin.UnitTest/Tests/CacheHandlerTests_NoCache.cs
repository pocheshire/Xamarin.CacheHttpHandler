using System;
using System.Net.Http;
using System.Threading.Tasks;
using CacheHandlerPlugin.Exceptions;
using CacheHandlerPlugin.UnitTest.Mocks;
using Xunit;

namespace CacheHandlerPlugin.UnitTest.Tests
{
    [Collection("UnitTest")]
    public class CacheHandlerTests_NoCache : BaseTests
    {

        public CacheHandlerTests_NoCache(UnitTestFixture fixture)
            : base(fixture) => Fixture.Reset();

        [Fact]
        public async Task CheckDataLoadedWithoutCache()
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                var responseMessage = await Fixture.HttpClient.SendAsync(requestMessage);

                Assert.False(Fixture.ApiCacheService.TryGet(requestMessage, out responseMessage));
            }
        }

        [Fact]
        public async Task CheckDataLoadedWithConnectivityException()
        {
            (Fixture.ConnectivityService as ConnectivityMock).IsConnected = false;
            
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                await Assert.ThrowsAsync<ConnectivityException>(async () => await Fixture.HttpClient.SendAsync(requestMessage));
            }

            (Fixture.ConnectivityService as ConnectivityMock).IsConnected = true;
        }

        [Fact]
        public async Task CheckDataLoadedWithException()
        {
            Fixture.MockHttpMessageHandler.IsBadRequest = true;
            Fixture.MockHttpMessageHandler.IsBadRequestWithException = true;

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                await Assert.ThrowsAnyAsync<Exception>(async () => await Fixture.HttpClient.SendAsync(requestMessage));
            }

            Fixture.MockHttpMessageHandler.IsBadRequest = false;
            Fixture.MockHttpMessageHandler.IsBadRequestWithException = false;
        }

        [Fact]
        public async Task CheckDataLoadedWithServerUnavailable()
        {
            Fixture.MockHttpMessageHandler.IsBadRequest = true;

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUri))
            {
                var responseMessage = await Fixture.HttpClient.SendAsync(requestMessage);

                Assert.True(responseMessage.StatusCode == Fixture.MockHttpMessageHandler.BadRequestCode);
            }

            Fixture.MockHttpMessageHandler.IsBadRequest = false;
        }
    }
}