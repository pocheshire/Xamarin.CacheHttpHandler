using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CacheHandlerPlugin.Realm.Models;
using CacheHandlerPlugin.Realm.Services.Repository;
using CacheHandlerPlugin.Services.ApiCache;

namespace CacheHandlerPlugin.Realm
{
    public class RealmApiCacheService : IApiCacheService
    {
        private readonly object _lockObject = new object();
        private readonly ConcurrentQueue<RealmCacheModel> _cachingQueue = new ConcurrentQueue<RealmCacheModel>();
        
        private bool _startingDequeuing;
        
        private IRealmRepository Repository { get; }

        public RealmApiCacheService(IRealmRepository repository)
        {
            Repository = repository;
        }

        private async Task<RealmCacheModel> CreateCacheModel(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage, TimeSpan expireIn)
        {
            var cacheResult = new RealmCacheRequestModel
            {
                Id = Guid.NewGuid().ToString(),
                Hash = requestMessage.Content == null ? string.Empty : GetHashString(await requestMessage.Content.ReadAsStringAsync().ConfigureAwait(false)),
                Date = DateTimeOffset.UtcNow,
                Content = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false)
            };

            foreach (var header in responseMessage.Headers)
            {
                var cacheHeader = new RealmCacheHeaderModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Key = header.Key
                };

                foreach (var headerValue in header.Value)
                {
                    cacheHeader.Value.Add(headerValue);
                }

                cacheResult.Headers.Add(cacheHeader);
            }

            var cacheModel = new RealmCacheModel
            {
                Key = GetKey(requestMessage),
                ExpirationInSeconds = expireIn.TotalSeconds
            };

            cacheModel.Results.Add(cacheResult);

            return cacheModel;
        }

        private static string GetKey(HttpRequestMessage requestMessage)
        {
            return requestMessage.RequestUri.ToString();
        }

        private static string GetHashString(string str)
        {
            string hash;
            using (var md5 = MD5.Create())
            {
                var hashedBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }

            return hash;
        }

        private Task StartDequeuing()
        {
            return Task.Run(() =>
            {
                _startingDequeuing = true;

                if (!_cachingQueue.TryDequeue(out var modelForCaching)) 
                    return;
                
                lock (_lockObject)
                {
                    var existingCache = Repository.Find<RealmCacheModel>(modelForCaching.Key);
                    if (existingCache != null)
                    {
                        var cachingRequestResult = modelForCaching.Results.Single();

                        var existingResult = existingCache.Results.FirstOrDefault(x => cachingRequestResult.Hash == x.Hash);
                        if (existingResult != null)
                        {
                            Repository.Update(() =>
                            {
                                existingResult.Date = cachingRequestResult.Date;
                                existingResult.Content = cachingRequestResult.Content;
                            });
                        }
                        else
                        {
                            Repository.Update(() =>
                            {
                                existingCache.Results.Add(cachingRequestResult);
                            });
                        }
                    }
                    else
                    {
                        Repository.Add(modelForCaching);
                    }
                }

                if (_cachingQueue.Count > 0)
                    StartDequeuing();
                else
                {
                    _startingDequeuing = false;
                }
            });
        }

        public async Task Add(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage, TimeSpan expireIn)
        {
            var cacheModel = await CreateCacheModel(requestMessage, responseMessage, expireIn).ConfigureAwait(false);
            
            _cachingQueue.Enqueue(cacheModel);

            if (!_startingDequeuing)
                await StartDequeuing().ConfigureAwait(false);
        }

        public bool TryGet(HttpRequestMessage requestMessage, out HttpResponseMessage responseMessage)
        {
            lock (_lockObject)
            {
                var cachedModel = Repository.Find<RealmCacheModel>(GetKey(requestMessage));
                if (cachedModel != null)
                {
                    var hash = string.Empty;

                    if (requestMessage.Content != null)
                    {
                        var postDataTask = requestMessage.Content.ReadAsStringAsync();
                        postDataTask.Wait();

                        hash = GetHashString(postDataTask.Result);
                    }

                    var cachedResult = cachedModel.Results.FirstOrDefault(x => x.Hash == hash);
                    if (cachedResult != null)
                    {
                        if ((DateTimeOffset.UtcNow - cachedResult.Date).TotalSeconds < cachedModel.ExpirationInSeconds)
                        {
                            responseMessage = new HttpResponseMessage
                            {
                                Content = new StringContent(cachedResult.Content)
                            };
                            
                            var headers = cachedResult.Headers.ToDictionary(x => x.Key, x => x.Value.AsEnumerable());
                            foreach (var header in headers)
                            {
                                responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                            }
                            
                            return true;
                        }

                        Repository.Remove(cachedModel);
                    }
                }
            }
            
            responseMessage = null;

            return false;
        }
    }
}
