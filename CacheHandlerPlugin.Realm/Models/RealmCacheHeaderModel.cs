using System.Collections.Generic;
using Realms;

namespace CacheHandlerPlugin.Realm.Models
{
    public class RealmCacheHeaderModel : RealmObject
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string Key { get; set; }

        public IList<string> Value { get; }
    }
}
