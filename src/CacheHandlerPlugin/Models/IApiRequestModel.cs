using System;
using System.Collections.Generic;

namespace CacheHandlerPlugin.Models
{
    public interface IApiRequestModel
    {
        string Id { get; set; }

        string Hash { get; set; }

        DateTimeOffset Date { get; set; }

        string Content { get; set; }

        IList<IApiResponseHeader> Headers { get; }
    }
}
