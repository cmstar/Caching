using System;
using StackExchange.Redis;

namespace cmstar.Caching.Redis
{
    /// <summary>
    /// 使用Redis作为缓存的缓存提供器。
    /// </summary>
    public class RedisCacheProvider : ICacheProvider
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly int _databaseNumber;

        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="connectionString">指定Redis的连接配置。</param>
        /// <param name="databaseNumber">指定缓存在Redis上使用的数据库编号。</param>
        public RedisCacheProvider(string connectionString, int databaseNumber = 0)
        {
            _redis = RedisConnectionCache.Get(connectionString);
            _databaseNumber = databaseNumber;
        }

        public T Get<T>(string key)
        {
            T value;
            TryGet(key, out value);
            return value;
        }

        public bool TryGet<T>(string key, out T value)
        {
            var db = _redis.GetDatabase(_databaseNumber);
            var redisValue = db.StringGet(key);

            if (!redisValue.HasValue)
            {
                value = default(T);
                return false;
            }

            var v = RedisConvert.FromRedisValue<T>(redisValue);
            value = v == null ? default(T) : (T)v;
            return true;
        }

        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            var e = TimeSpan.Zero.Equals(expiration) ? (TimeSpan?)null : expiration;
            var db = _redis.GetDatabase(_databaseNumber);
            var redisValue = RedisConvert.ToRedisValue(value);
            db.StringSet(key, redisValue, e);
        }

        public bool Remove(string key)
        {
            var db = _redis.GetDatabase(_databaseNumber);
            return db.KeyDelete(key);
        }
    }
}
