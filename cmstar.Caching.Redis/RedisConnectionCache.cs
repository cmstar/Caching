using System.Collections.Concurrent;
using StackExchange.Redis;

namespace cmstar.Caching.Redis
{
    internal static class RedisConnectionCache
    {
        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> Cache
            = new ConcurrentDictionary<string, ConnectionMultiplexer>();

        public static ConnectionMultiplexer Get(string configuration)
        {
            var multiplexer = Cache.GetOrAdd(configuration, CreateMultiplexer);
            return multiplexer;
        }

        private static ConnectionMultiplexer CreateMultiplexer(string configuration)
        {
            var multiplexer = ConnectionMultiplexer.Connect(configuration);
            return multiplexer;
        }
    }
}
