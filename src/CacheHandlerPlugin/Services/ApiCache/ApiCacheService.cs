using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CacheHandlerPlugin.Models;
using CacheHandlerPlugin.Services.Repository;

namespace CacheHandlerPlugin.Services.ApiCache
{
    public abstract class ApiCacheService<T> : IApiCacheService
        where T : class, IApiCacheModel
    {
        private readonly ConcurrentQueue<T> _cachingQueue = new ConcurrentQueue<T>();

        protected IRepository Repository { get; }

        protected ApiCacheService(IRepository repository)
        {
            Repository = repository;
        }

        public void Add(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage, TimeSpan expireIn)
        {
            var cacheModel = CreateCacheModel(requestMessage, responseMessage, expireIn);

            _cachingQueue.Enqueue(cacheModel);

            if (!_cachingQueue.IsEmpty)
                StartDequeuing();
        }

        public bool TryGet(HttpRequestMessage requestMessage, out HttpResponseMessage responseMessage)
        {
            responseMessage = new HttpResponseMessage();

            var cachedModel = Find(GetKey(requestMessage));
            if (cachedModel != null)
            {
                //var hash = string.Empty;
                //if (requestMessage.Content != null)
                //{
                //    var postDataTask = requestMessage.Content.ReadAsStringAsync();
                //    postDataTask.Wait();
                //    hash = MD5Helper.GetHashString(postDataTask.Result);
                //}

                var cachedResult = FindCachedResult(cachedModel.Results, requestMessage);
                if (cachedResult != null)
                {
                    if ((DateTimeOffset.UtcNow - cachedResult.Date).TotalSeconds < cachedModel.ExpirationInSeconds)
                    {
                        responseMessage.StatusCode = System.Net.HttpStatusCode.OK;
                        responseMessage.Content = new StringContent(cachedResult.Content);

                        foreach (var header in cachedResult.Headers.ToDictionary(x => x.Key, x => x.Value.AsEnumerable()))
                        {
                            responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }

                        return true;
                    }

                    Remove(cachedModel);
                }
            }

            return false;
        }

        protected virtual T Find(string key)
        {
            return Repository.Find<T>(key);
        }

        protected virtual void Add(T cachedModel)
        {
            Repository.Add(cachedModel);
        }

        protected virtual void Update(Action updateAction)
        {
            Repository.Update(updateAction);
        }

        protected virtual void Remove(T cachedModel)
        {
            Repository.Remove(cachedModel);
        }

        protected virtual void RemoveAll()
        {
            Repository.RemoveAll<T>();
        }

        private string GetKey(HttpRequestMessage requestMessage)
        {
            return requestMessage.RequestUri.ToString();
        }

        protected abstract IApiRequestModel FindCachedResult(IList<IApiRequestModel> results, HttpRequestMessage requestMessage);

        protected abstract T CreateCacheModel(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage, TimeSpan expireIn);
        /*
        {
            var cacheResult = new ApiRequestModel
            {
                Id = Guid.NewGuid().ToString(),
                Hash = requestMessage.Content == null ? string.Empty : MD5Helper.GetHashString(await requestMessage.Content.ReadAsStringAsync()),
                Date = DateTimeOffset.UtcNow,
                Content = await responseMessage.Content.ReadAsStringAsync()
            };

            foreach (var header in responseMessage.Headers)
            {
                var cacheHeader = new ApiResponseHeader
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

            var cacheModel = new IApiCacheModel
            {
                Key = GetKey(requestMessage),
                ExpirationInSeconds = expireIn.TotalSeconds
            };

            cacheModel.Results.Add(cacheResult);

            return cacheModel;
        }
        */

        private void StartDequeuing()
        {
            T modelForCaching = null;

            if (!_cachingQueue.TryDequeue(out modelForCaching))
                return;

            if (modelForCaching != null)
            {
                var existingCache = Find(modelForCaching.Key);
                if (existingCache != null)
                {
                    var cachingRequestResult = modelForCaching.Results.Single();

                    var existingResult = existingCache.Results.FirstOrDefault(x => cachingRequestResult.Hash == x.Hash);
                    if (existingResult != null)
                    {
                        Update(() =>
                        {
                            existingResult.Date = cachingRequestResult.Date;
                            existingResult.Content = cachingRequestResult.Content;
                        });
                    }
                    else
                    {
                        Update(() =>
                        {
                            existingCache.Results.Add(cachingRequestResult);
                        });
                    }
                }
                else
                {
                    Add(modelForCaching);
                }

                if (!_cachingQueue.IsEmpty)
                    StartDequeuing();
            }
        }
    }
}
