using System.Collections.Concurrent;
using StackExchange.Redis;

namespace cmstar.Caching.Redis
{
    public static class RedisConnectionCache
    {
        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> Cache
            = new ConcurrentDictionary<string, ConnectionMultiplexer>();

        public static ConnectionMultiplexer Get(string configuration)
        {
            ConnectionMultiplexer multiplexer;
            if (Cache.TryGetValue(configuration, out multiplexer))
                return multiplexer;

            lock (Cache)
            {
                if (Cache.TryGetValue(configuration, out multiplexer))
                    return multiplexer;

                multiplexer = ConnectionMultiplexer.Connect(configuration);
                Cache.TryAdd(configuration, multiplexer);
            }

            return multiplexer;
        }
    }
}
