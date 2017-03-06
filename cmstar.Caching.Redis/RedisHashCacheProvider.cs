using System;
using StackExchange.Redis;

namespace cmstar.Caching.Redis
{
    /// <summary>
    /// 基于redis的hash实现的缓存提供器。
    /// </summary>
    public class RedisHashCacheProvider : ICacheFieldIncreasable, ICacheIncreasable
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
            CreateOrSet(key, value, expiration, false);
        }

        public bool Create<T>(string key, T value, TimeSpan expiration)
        {
            return CreateOrSet(key, value, expiration, true);
        }

        public bool Remove(string key)
        {
            var db = _redis.GetDatabase(_databaseNumber);
            return db.KeyDelete(key);
        }

        public T Increase<T>(string key, T increment)
        {
            var incrementLong = Convert.ToInt64(increment);
            var db = _redis.GetDatabase(_databaseNumber);
            var tran = db.CreateTransaction();
            tran.AddCondition(Condition.KeyExists(key));
            var res = tran.HashIncrementAsync(key, RedisConvert.EntryNameForSpecialValue, incrementLong);
            return tran.Execute()
                ? (T)Convert.ChangeType(res.Result, typeof(T))
                : default(T);
        }

        public T IncreaseOrCreate<T>(string key, T increment, TimeSpan expiration)
        {
            var incrementLong = Convert.ToInt64(increment);
            var db = _redis.GetDatabase(_databaseNumber);
            var res = db.HashIncrement(key, RedisConvert.EntryNameForSpecialValue, incrementLong);

            // 超时的处理采用和RedisCacheProvider.IncreaseOrCreate相同的方式
            if (res == incrementLong && !TimeSpan.Zero.Equals(expiration))
            {
                db.KeyExpire(key, expiration);
            }

            return (T)Convert.ChangeType(res, typeof(T));
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

            var v = RedisConvert.FromRedisValue<TField>(redisValue);
            value = v == null ? default(TField) : (TField)v;
            return true;
        }

        public bool FieldSet<T, TField>(string key, string field, TField value)
        {
            var redisValue = RedisConvert.ToRedisValue(value);
            var db = _redis.GetDatabase(_databaseNumber);

            var tran = db.CreateTransaction();
            tran.AddCondition(Condition.HashExists(key, field));
            tran.HashSetAsync(key, field, redisValue);
            var res = tran.Execute();
            return res;
        }

        public bool FieldSetOrCreate<T, TField>(string key, string field, TField value, TimeSpan expiration)
        {
            var redisValue = RedisConvert.ToRedisValue(value);
            var db = _redis.GetDatabase(_databaseNumber);

            if (db.HashSet(key, field, redisValue))
            {
                db.KeyExpire(key, expiration);
            }

            return true;
        }

        public TField FieldIncrease<T, TField>(string key, string field, TField increment)
        {
            var incrementLong = Convert.ToInt64(increment);
            var db = _redis.GetDatabase(_databaseNumber);
            var tran = db.CreateTransaction();
            tran.AddCondition(Condition.HashExists(key, field));

            var res = tran.HashIncrementAsync(key, field, incrementLong);
            return tran.Execute()
                ? (TField)Convert.ChangeType(res.Result, typeof(TField))
                : default(TField);
        }

        public TField FieldIncreaseOrCreate<T, TField>(string key, string field, TField increment, TimeSpan expiration)
        {
            var incrementLong = Convert.ToInt64(increment);
            var db = _redis.GetDatabase(_databaseNumber);
            var res = db.HashIncrement(key, field, incrementLong);

            // 超时的处理采用和RedisCacheProvider.IncreaseOrCreate相同的方式
            if (res == incrementLong && !TimeSpan.Zero.Equals(expiration))
            {
                db.KeyExpire(key, expiration);
            }

            return (TField)Convert.ChangeType(res, typeof(TField));
        }

        private bool CreateOrSet<T>(string key, T value, TimeSpan expiration, bool create)
        {
            bool shouldRemoveSpecialEntry;
            var hashEntries = RedisConvert.ToHashEntries(value, out shouldRemoveSpecialEntry);
            var e = TimeSpan.Zero.Equals(expiration) ? (TimeSpan?)null : expiration;
            var db = _redis.GetDatabase(_databaseNumber);
            var tran = db.CreateTransaction();

            if (create)
            {
                tran.AddCondition(Condition.KeyNotExists(key));
            }

            if (shouldRemoveSpecialEntry)
            {
                tran.HashDeleteAsync(key, RedisConvert.EntryNameForSpecialValue);
            }

            tran.HashSetAsync(key, hashEntries);
            tran.KeyExpireAsync(key, e);
            return tran.Execute();
        }
    }
}