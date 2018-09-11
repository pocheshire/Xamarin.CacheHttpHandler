using System;
using System.Threading.Tasks;
using CacheHandlerPlugin;
using System.Net.Http;
using CacheHandlerPlugin.Realm;
using CacheHandlerPlugin.Connectivity;
using CacheHandlerPlugin.Realm.Services.Repository;
using CacheHandlerPlugin.Models;
using Sample.NetApp.Cache;

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

            var netAppCSC = new NetAppCSC();

            var handler = new CacheMessageHandler(
                new HttpClientHandler(),
                apiCacheService,
                new ConnectivityService())
            {
                CacheSettingsContainer = netAppCSC
            };

            var httpClient = new HttpClient(handler);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Constants.BaseUrl}/people/"))
            {
                System.Console.WriteLine("Register cache policy...");

                netAppCSC.Register(requestMessage, new RequestCacheSettings(600));

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
