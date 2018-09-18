using System.Net.Http;
using System.Threading.Tasks;
using CacheHandlerPlugin.Models;
using CacheHandlerPlugin.Services.CacheSettings;
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
    }
}
