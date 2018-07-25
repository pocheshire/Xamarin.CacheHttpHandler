using System.Collections.Generic;
using CacheHandlerPlugin.Models;
using Realms;

namespace CacheHandlerPlugin.Realm.Models
{
    public class RealmResponseHeader : RealmObject, IApiResponseHeader
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string Key { get; set; }

        public IList<string> Value { get; }
    }
}
