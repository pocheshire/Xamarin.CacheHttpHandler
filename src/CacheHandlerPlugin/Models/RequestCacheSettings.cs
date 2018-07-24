namespace CacheHandlerPlugin.Models
{
    public class RequestCacheSettings
    {
        public RequestCacheSettings()
        {

        }

        public RequestCacheSettings(int expireInSeconds, bool preferLoadFromCache = false)
        {
            IsCacheable = true;
            ExpireInSeconds = expireInSeconds;
            Policy = preferLoadFromCache ? CachePolicy.CacheFirst : CachePolicy.RequestFirst;
        }

        public bool IsCacheable { get; set; }

        public int ExpireInSeconds { get; set; }

        public CachePolicy Policy { get; set; }

        public static RequestCacheSettings Default => new RequestCacheSettings { IsCacheable = false };
    }
}
