using System.Net.Http;
using System.Threading.Tasks;
using CacheHandlerPlugin.Models;
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
    }
}
