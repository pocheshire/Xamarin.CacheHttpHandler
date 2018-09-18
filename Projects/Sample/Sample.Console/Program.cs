using System.Net.Http;
using System.Threading.Tasks;
using CacheHandlerPlugin;
using CacheHandlerPlugin.Connectivity;
using CacheHandlerPlugin.Models;
using CacheHandlerPlugin.Realm;
using CacheHandlerPlugin.Realm.Services.Repository;
using CacheHandlerPlugin.Services.CacheSettings;

namespace Sample.NetApp
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Start!");

            SendRequestWithSettings().GetAwaiter().GetResult();

            SendRequestWithPolicyRegistration().GetAwaiter().GetResult();
        }

        private static async Task SendRequestWithSettings()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("Request with settings in message properties!");

            var apiCacheService = new RealmApiCacheService(new RealmRepository(null));

            var handler = new CacheMessageHandler(
                new HttpClientHandler(),
                apiCacheService,
                new ConnectivityService());

            var httpClient = new HttpClient(handler);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Constants.BaseUrl}/people/"))
            {
                System.Console.WriteLine("Request sending...");

                requestMessage.Properties.Add(CacheMessageHandler.CachedKey, new RequestCacheSettings(600));

                var responseMessage = await httpClient.SendAsync(requestMessage);

                if (apiCacheService.TryGet(requestMessage, out var cachedResponseMessage))
                {
                    System.Console.WriteLine($"Content from network: {await responseMessage.Content.ReadAsStringAsync()}");
                    System.Console.WriteLine();
                    System.Console.WriteLine($"Content from cache: {await cachedResponseMessage.Content.ReadAsStringAsync()}");
                }
                else
                {
                    System.Console.WriteLine("Something went wrong!");
                }
            }
        }

        private static async Task SendRequestWithPolicyRegistration()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("Request with cache settings registration!");

            var apiCacheService = new RealmApiCacheService(new RealmRepository(null));

            var csc = new SimpleCacheSettingsContainer();

            var handler = new CacheMessageHandler(
                new HttpClientHandler(),
                apiCacheService,
                new ConnectivityService())
            {
                CacheSettingsContainer = csc
            };

            var httpClient = new HttpClient(handler);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Constants.BaseUrl}/people/"))
            {
                System.Console.WriteLine("Register cache policy...");

                csc.Register(requestMessage, new RequestCacheSettings(600));

                System.Console.WriteLine("Request sending...");

                var responseMessage = await httpClient.SendAsync(requestMessage);

                if (apiCacheService.TryGet(requestMessage, out var cachedResponseMessage))
                {
                    System.Console.WriteLine($"Content from network: {await responseMessage.Content.ReadAsStringAsync()}");
                    System.Console.WriteLine();
                    System.Console.WriteLine($"Content from cache: {await cachedResponseMessage.Content.ReadAsStringAsync()}");
                }
                else
                {
                    System.Console.WriteLine("Something went wrong!");
                }
            }
        }
    }
}
