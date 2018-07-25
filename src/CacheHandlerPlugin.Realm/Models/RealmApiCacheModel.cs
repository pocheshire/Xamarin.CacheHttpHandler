using System.Collections.Generic;
using CacheHandlerPlugin.Models;
using Realms;

namespace CacheHandlerPlugin.Realm.Models
{
    public class RealmApiCacheModel : RealmObject, IApiCacheModel
    {
        [PrimaryKey]
        public string Key { get; set; }

        public IList<IApiRequestModel> Results { get; }

        public double ExpirationInSeconds { get; set; }
    }
}
