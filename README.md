# Xamarin.CacheHttpHandler

## Usage

0. Install packages [CacheHandlerPlugin](https://www.nuget.org/packages/CacheHandlerPlugin/), [CacheHandlerPlugin.Realm](https://www.nuget.org/packages/CacheHandlerPlugin.Realm/), [CacheHandlerPlugin.Connectivity](https://www.nuget.org/packages/CacheHandlerPlugin.Connectivity/)

1. Create handler instance

    ```csharp
    var handler = new CacheMessageHandler(
                %your_current_handler_instance%,
                apiCacheService,
                connectionService);
    ```

    where

    ```csharp
    var apiCacheService = new RealmApiCacheService(new RealmRepository(null));
    ```

    or your own `IApiCacheService` implementation

    and
    
    ```csharp
    var connectionService = new ConnectivityService();
    ```

    or your own `IConnectivityService` implementation

2. Create HttpClient instance

    ```csharp
    var httpClient = new HttpClient(handler);
    ```

3. Register `RequestCacheSettings` in your own `ICacheSettingsContainer` implementation or add cache settings in request message properties

    ```csharp
    var cacheSettingsContainer = new NetAppCSC();
    
    var requestMessage = new HttpRequestMessage(HttpMethod.Get, %url%);

    netAppCSC.Register(requestMessage, new RequestCacheSettings(600));
    ```

    or

    ```csharp
    var requestMessage = new HttpRequestMessage(HttpMethod.Get, %url%);
    requestMessage.Properties.Add(CacheMessageHandler.CachedKey, new RequestCacheSettings(600));
    ```

4. ...

5. PROFIT!