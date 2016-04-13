using System.Collections.Concurrent;
using StackExchange.Redis;

namespace cmstar.Caching.Redis
{
    /// <summary>
    /// 提供每个redis配置信息对应到<see cref="IConnectionMultiplexer"/>的单例。
    /// </summary>
    internal static class RedisConnectionCache
    {
        private static readonly ConcurrentDictionary<string, IConnectionMultiplexer> Cache
            = new ConcurrentDictionary<string, IConnectionMultiplexer>();

        /// <summary>
        /// 从redis配置信息获取<see cref="IConnectionMultiplexer"/>的实例。
        /// </summary>
        /// <param name="configuration">配置信息。</param>
        /// <returns></returns>
        public static IConnectionMultiplexer Get(string configuration)
        {
            var multiplexer = Cache.GetOrAdd(configuration, CreateMultiplexer);
            return multiplexer;
        }

        private static IConnectionMultiplexer CreateMultiplexer(string configuration)
        {
            var multiplexer = ConnectionMultiplexer.Connect(configuration);
            return multiplexer;
        }
    }
}

