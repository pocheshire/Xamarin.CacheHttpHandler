using System;
using System.Net.Http;

namespace CacheHandlerPlugin.Exceptions
{
    public class ConnectivityException : Exception
    {
        public HttpRequestMessage RequestMessage { get; }

        public ConnectivityException()
        {
        }

        public ConnectivityException(HttpRequestMessage requestMessage)
            => RequestMessage = requestMessage;
    }
}
