using System.Collections.Generic;

namespace CacheHandlerPlugin.Models
{
    public interface IApiCacheModel
    {
        string Key { get; set; }

        IList<IApiRequestModel> Results { get; }

        double ExpirationInSeconds { get; set; }
    }
}
