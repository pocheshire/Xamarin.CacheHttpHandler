using System;
using System.Collections.Generic;
using CacheHandlerPlugin.Models;
using Realms;

namespace CacheHandlerPlugin.Realm.Models
{
    public class RealmApiRequestModel : RealmObject, IApiRequestModel
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string Hash { get; set; }

        public DateTimeOffset Date { get; set; }

        public string Content { get; set; }

        public IList<IApiResponseHeader> Headers { get; }
    }
}
