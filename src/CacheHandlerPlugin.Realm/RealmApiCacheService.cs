using System;
using System.Collections.Generic;
using System.Net.Http;
using CacheHandlerPlugin.Models;
using CacheHandlerPlugin.Realm.Models;
using CacheHandlerPlugin.Services.ApiCache;
using CacheHandlerPlugin.Services.Repository;

namespace CacheHandlerPlugin.Realm
{
    public class RealmApiCacheService : ApiCacheService<RealmApiCacheModel>
    {
        public RealmApiCacheService(IRepository repository)
            : base (repository)
        {
        }

        protected override RealmApiCacheModel CreateCacheModel(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage, TimeSpan expireIn)
        {
            throw new NotImplementedException();
        }

        protected override IApiRequestModel FindCachedResult(IList<IApiRequestModel> results, HttpRequestMessage requestMessage)
        {
            throw new NotImplementedException();
        }
    }
}
