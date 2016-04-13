using System;
using StackExchange.Redis;

namespace cmstar.Caching.Redis
{
    /// <summary>
    /// 基于redis的hash实现的缓存提供器。
    /// </summary>
    public class RedisHashCacheProvider : ICacheFieldAccessable
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly int _databaseNumber;

        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="connectionString">指定Redis的连接配置。</param>
        /// <param name="databaseNumber">指定缓存在Redis上使用的数据库编号。</param>
        public RedisHashCacheProvider(string connectionString, int databaseNumber = 0)
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
            if (!db.KeyExists(key))
            {
                value = default(T);
                return false;
            }

            var hashEntries = db.HashScan(key);
            value = RedisConvert.FromCacheEntries<T>(hashEntries);
            return true;
        }

        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            var hashEntries = RedisConvert.ToHashEntries(value);
            var e = TimeSpan.Zero.Equals(expiration) ? (TimeSpan?)null : expiration;
            var db = _redis.GetDatabase(_databaseNumber);
            var tran = db.CreateTransaction();

            var task1 = tran.HashSetAsync(key, hashEntries);
            var task2 = tran.KeyExpireAsync(key, e);
            tran.Execute();
            tran.WaitAll(task1, task2);
        }

        public bool Remove(string key)
        {
            var db = _redis.GetDatabase(_databaseNumber);
            return db.KeyDelete(key);
        }

        public TField FieldGet<T, TField>(string key, string field)
        {
            TField value;
            FieldTryGet<T, TField>(key, field, out value);
            return value;
        }

        public bool FieldTryGet<T, TField>(string key, string field, out TField value)
        {
            var db = _redis.GetDatabase(_databaseNumber);
            var redisValue = db.HashGet(key, field);

            if (!redisValue.HasValue)
            {
                value = default(TField);
                return false;
            }

            value = RedisConvert.FromRedisValue<TField>(redisValue);
            return true;
        }

        public bool FieldSet<T, TField>(string key, string field, TField value)
        {
            var redisValue = RedisConvert.ToRedisValue(value);
            var db = _redis.GetDatabase(_databaseNumber);

            var tran = db.CreateTransaction();
            tran.AddCondition(Condition.HashExists(key, field));
            var t = tran.HashSetAsync(key, field, redisValue);
            var res = tran.Execute();

            tran.Wait(t);
            return res;
        }

        public bool FieldSetCx<T, TField>(string key, string field, TField value, TimeSpan expiration)
        {
            var redisValue = RedisConvert.ToRedisValue(value);
            var db = _redis.GetDatabase(_databaseNumber);

            if (db.HashSet(key, field, redisValue))
            {
                db.KeyExpire(key, expiration);
            }

            return true;
        }
    }
}
