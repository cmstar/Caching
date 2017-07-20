using System;
using StackExchange.Redis;

namespace cmstar.Caching.Redis
{
    /// <summary>
    /// 使用Redis作为缓存的缓存提供器。
    /// </summary>
    public class RedisCacheProvider : ICacheIncreasable
    {
        private readonly IConnectionMultiplexer _redis;
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

        /// <inheritdoc cref="ICacheProvider.Get{T}" />
        public T Get<T>(string key)
        {
            T value;
            TryGet(key, out value);
            return value;
        }

        /// <inheritdoc cref="ICacheProvider.TryGet{T}" />
        public bool TryGet<T>(string key, out T value)
        {
            var db = _redis.GetDatabase(_databaseNumber);
            var redisValue = db.StringGet(key);

            if (redisValue.IsNull)
            {
                value = default(T);
                return false;
            }

            var v = RedisConvert.FromRedisValue<T>(redisValue);
            value = v == null ? default(T) : (T)v;
            return true;
        }

        /// <inheritdoc cref="ICacheProvider.Set{T}" />
        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            InternalSet(key, value, expiration, When.Always);
        }

        /// <inheritdoc cref="ICacheProvider.Create{T}" />
        public bool Create<T>(string key, T value, TimeSpan expiration)
        {
            return InternalSet(key, value, expiration, When.NotExists);
        }

        /// <inheritdoc cref="ICacheProvider.Remove" />
        public bool Remove(string key)
        {
            var db = _redis.GetDatabase(_databaseNumber);
            return db.KeyDelete(key);
        }

        /// <inheritdoc cref="ICacheIncreasable.Increase{T}" />
        public T Increase<T>(string key, T increment)
        {
            var incrementLong = Convert.ToInt64(increment);
            var db = _redis.GetDatabase(_databaseNumber);

            var tran = db.CreateTransaction();
            tran.AddCondition(Condition.KeyExists(key));

            var res = tran.StringIncrementAsync(key, incrementLong);
            return tran.Execute()
                ? (T)Convert.ChangeType(res.Result, typeof(T))
                : default(T);
        }

        /// <inheritdoc cref="ICacheIncreasable.IncreaseOrCreate{T}" />
        public T IncreaseOrCreate<T>(string key, T increment, TimeSpan expiration)
        {
            var incrementLong = Convert.ToInt64(increment);
            var db = _redis.GetDatabase(_databaseNumber);
            var res = db.StringIncrement(key, incrementLong);

            /*
             * redis的INCR*命令本身没有提供仅在新建值的时候设置过期时间的机制，
             * 这里分两步处理，先INCR*；然后判定如果是新建的值，就设置过期时间。
             * 然而INCR*仅返回当前的值，不说明值是新建的还是更新的，我们等通过
             * 返回的值是否与增量一致来判断是否是新建的。
             */
            if (res == incrementLong && !TimeSpan.Zero.Equals(expiration))
            {
                db.KeyExpire(key, expiration);
            }

            return (T)Convert.ChangeType(res, typeof(T));
        }

        private bool InternalSet<T>(string key, T value, TimeSpan expiration, When when)
        {
            var e = TimeSpan.Zero.Equals(expiration) ? (TimeSpan?)null : expiration;
            var db = _redis.GetDatabase(_databaseNumber);
            var redisValue = RedisConvert.ToRedisValue(value);
            return db.StringSet(key, redisValue, e, when);
        }
    }
}