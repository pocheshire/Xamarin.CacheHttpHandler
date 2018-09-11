using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using CacheHandlerPlugin;
using CacheHandlerPlugin.Connectivity;
using CacheHandlerPlugin.Models;
using CacheHandlerPlugin.Realm;
using CacheHandlerPlugin.Realm.Services.Repository;
using Playground.Core.Cache;

namespace Playground.Core
{
    public class MainViewModel
    {
        private ICommand _sendRequestCommand;
        public ICommand SendRequestCommand => _sendRequestCommand ?? (_sendRequestCommand = new SimpleCommand(OnSendRequestExecute));

        private async void OnSendRequestExecute()
        {
            await SendRequestWithSettings();
            //await SendRequestWithPolicyRegistration();
        }

        private async Task SendRequestWithSettings()
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

        private async Task SendRequestWithPolicyRegistration()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("Request with cache settings registration!");

            var apiCacheService = new RealmApiCacheService(new RealmRepository(null));

            var netAppCSC = new CacheSettingsContainer();

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

        internal class SimpleCommand : ICommand
        {
            private readonly Action _onSendRequestExecute;

            public SimpleCommand(Action onSendRequestExecute)
            {
                _onSendRequestExecute = onSendRequestExecute;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                _onSendRequestExecute?.Invoke();
            }
        }
    }
}
