using System;
using System.Collections.Generic;
using Realms;

namespace CacheHandlerPlugin.Realm.Models
{
    public class RealmCacheRequestModel : RealmObject
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string Hash { get; set; }

        public DateTimeOffset Date { get; set; }

        public string Content { get; set; }

        public IList<RealmCacheHeaderModel> Headers { get; }
    }
}
