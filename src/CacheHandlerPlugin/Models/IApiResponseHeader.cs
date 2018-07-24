using System.Collections.Generic;

namespace CacheHandlerPlugin.Models
{
    public interface IApiResponseHeader
    {
        string Id { get; set; }

        string Key { get; set; }

        IList<string> Value { get; }
    }
}
