using System.Collections.Generic;
using Realms;

namespace CacheHandlerPlugin.Realm.Models
{
    public class RealmCacheModel : RealmObject
    {
        [PrimaryKey]
        public string Key { get; set; }

        public IList<RealmCacheRequestModel> Results { get; }

        public double ExpirationInSeconds { get; set; }
    }
}
