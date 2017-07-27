﻿using System;
using System.Threading.Tasks;
using cmstar.Caching;
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
            if (!db.KeyExists(key))
            {
                value = default(T);
                return false;
            }

            var hashEntries = db.HashScan(key);
            value = RedisConvert.FromCacheEntries<T>(hashEntries);
            return true;
        }

        /// <inheritdoc cref="ICacheProvider.Set{T}" />
        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            CreateOrSet(key, value, expiration, false);
        }

        /// <inheritdoc cref="ICacheProvider.Create{T}" />
        public bool Create<T>(string key, T value, TimeSpan expiration)
        {
            return CreateOrSet(key, value, expiration, true);
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
            var res = tran.HashIncrementAsync(key, RedisConvert.EntryNameForSpecialValue, incrementLong);
            return tran.Execute()
                ? (T)Convert.ChangeType(res.Result, typeof(T))
                : default(T);
        }

        /// <inheritdoc cref="ICacheIncreasable.IncreaseOrCreate{T}" />
        public T IncreaseOrCreate<T>(string key, T increment, TimeSpan expiration)
        {
            var incrementLong = Convert.ToInt64(increment);
            var db = _redis.GetDatabase(_databaseNumber);
            var res = db.HashIncrement(key, RedisConvert.EntryNameForSpecialValue, incrementLong);

            // 超时的处理采用和RedisCacheProvider.IncreaseOrCreate相同的方式
            if (!TimeSpan.Zero.Equals(expiration) && RedisConvert.IsNewlyCreatedAfterIncreasing(res, incrementLong))
            {
                db.KeyExpire(key, expiration);
            }

            return (T)Convert.ChangeType(res, typeof(T));
        }

        /// <inheritdoc cref="ICacheFieldAccessable.FieldGet{T,TField}" />
        public TField FieldGet<T, TField>(string key, string field)
        {
            TField value;
            FieldTryGet<T, TField>(key, field, out value);
            return value;
        }

        /// <inheritdoc cref="ICacheFieldAccessable.FieldTryGet{T,TField}" />
        public bool FieldTryGet<T, TField>(string key, string field, out TField value)
        {
            var db = _redis.GetDatabase(_databaseNumber);
            var redisValue = db.HashGet(key, field);

            if (redisValue.IsNull)
            {
                value = default(TField);
                return false;
            }

            value = RedisConvert.FromRedisValue<TField>(redisValue);
            return true;
        }

        /// <inheritdoc cref="ICacheFieldAccessable.FieldSet{T,TField}" />
        public bool FieldSet<T, TField>(string key, string field, TField value)
        {
            var tran = PrepareTransationForFieldSet(key, field, value);
            return tran.Execute();
        }

        /// <inheritdoc cref="ICacheFieldAccessable.FieldSetOrCreate{T,TField}" />
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

        /// <inheritdoc cref="ICacheFieldIncreasable.FieldIncrease{T,TField}" />
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

        /// <inheritdoc cref="ICacheFieldIncreasable.FieldIncreaseOrCreate{T,TField}" />
        public TField FieldIncreaseOrCreate<T, TField>(string key, string field, TField increment, TimeSpan expiration)
        {
            var incrementLong = Convert.ToInt64(increment);
            var db = _redis.GetDatabase(_databaseNumber);
            var res = db.HashIncrement(key, field, incrementLong);

            // 超时的处理采用和RedisCacheProvider.IncreaseOrCreate相同的方式
            if (!TimeSpan.Zero.Equals(expiration) && RedisConvert.IsNewlyCreatedAfterIncreasing(res, incrementLong))
            {
                db.KeyExpire(key, expiration);
            }

            return (TField)Convert.ChangeType(res, typeof(TField));
        }

        /// <inheritdoc cref="ICacheProvider.GetAsync{T}" />
        public async Task<T> GetAsync<T>(string key)
        {
            var res = await TryGetAsync<T>(key);
            return res.Value;
        }

        /// <inheritdoc cref="ICacheProvider.TryGetAsync{T}" />
        public async Task<TryGetResult<T>> TryGetAsync<T>(string key)
        {
            var db = _redis.GetDatabase(_databaseNumber);
            var hasValue = await db.KeyExistsAsync(key);

            T value;
            if (hasValue)
            {
                var hashEntries = db.HashScan(key);
                value = RedisConvert.FromCacheEntries<T>(hashEntries);
            }
            else
            {
                value = default(T);
            }

            return new TryGetResult<T>(hasValue, value);
        }

        /// <inheritdoc cref="ICacheProvider.SetAsync{T}" />
        public Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            return CreateOrSetAsync(key, value, expiration, false);
        }

        /// <inheritdoc cref="ICacheProvider.CreateAsync{T}" />
        public Task<bool> CreateAsync<T>(string key, T value, TimeSpan expiration)
        {
            return CreateOrSetAsync(key, value, expiration, true);
        }

        /// <inheritdoc cref="ICacheProvider.RemoveAsync" />
        public Task<bool> RemoveAsync(string key)
        {
            var db = _redis.GetDatabase(_databaseNumber);
            return db.KeyDeleteAsync(key);
        }

        /// <inheritdoc cref="ICacheIncreasable.IncreaseAsync{T}" />
        public async Task<T> IncreaseAsync<T>(string key, T increment)
        {
            var incrementLong = Convert.ToInt64(increment);
            var db = _redis.GetDatabase(_databaseNumber);
            var tran = db.CreateTransaction();
            tran.AddCondition(Condition.KeyExists(key));

            var incrTask = tran.HashIncrementAsync(key, RedisConvert.EntryNameForSpecialValue, incrementLong);
            var tranSucc = await tran.ExecuteAsync();

            if (!tranSucc)
                return default(T);

            // 根据SE.Redis的文档，await ExecuteAsync 之后，事务内的操作肯定都完成了，
            // 所以这里其实是可以访问 incrTask.Result 的。但我们应当尽量避免在 async
            // 代码内这样做，所以这里仍然 await 之。
            return (T)Convert.ChangeType(await incrTask, typeof(T));
        }

        /// <inheritdoc cref="ICacheIncreasable.IncreaseOrCreateAsync{T}" />
        public async Task<T> IncreaseOrCreateAsync<T>(string key, T increment, TimeSpan expiration)
        {
            var incrementLong = Convert.ToInt64(increment);
            var db = _redis.GetDatabase(_databaseNumber);
            var res = await db.HashIncrementAsync(key, RedisConvert.EntryNameForSpecialValue, incrementLong);

            // 超时的处理采用和RedisCacheProvider.IncreaseOrCreate相同的方式
            if (!TimeSpan.Zero.Equals(expiration) && RedisConvert.IsNewlyCreatedAfterIncreasing(res, incrementLong))
            {
                db.KeyExpire(key, expiration);
            }

            return (T)Convert.ChangeType(res, typeof(T));
        }

        /// <inheritdoc cref="ICacheFieldAccessable.FieldGetAsync{T,TField}"/>
        public async Task<TField> FieldGetAsync<T, TField>(string key, string field)
        {
            var res = await FieldTryGetAsync<T, TField>(key, field);
            return res.Value;
        }

        /// <inheritdoc cref="ICacheFieldAccessable.FieldTryGetAsync{T,TField}"/>
        public async Task<TryGetResult<TField>> FieldTryGetAsync<T, TField>(string key, string field)
        {
            var db = _redis.GetDatabase(_databaseNumber);
            var redisValue = await db.HashGetAsync(key, field);

            var hasValue = !redisValue.IsNull;
            var value = hasValue
                ? RedisConvert.FromRedisValue<TField>(redisValue)
                : default(TField);

            return new TryGetResult<TField>(hasValue, value);
        }

        /// <inheritdoc cref="ICacheFieldAccessable.FieldSetAsync{T,TField}"/>
        public Task<bool> FieldSetAsync<T, TField>(string key, string field, TField value)
        {
            var tran = PrepareTransationForFieldSet(key, field, value);
            return tran.ExecuteAsync();
        }

        /// <inheritdoc cref="ICacheFieldAccessable.FieldSetOrCreateAsync{T,TField}"/>
        public async Task<bool> FieldSetOrCreateAsync<T, TField>(string key, string field, TField value, TimeSpan expiration)
        {
            var redisValue = RedisConvert.ToRedisValue(value);
            var db = _redis.GetDatabase(_databaseNumber);

            if (await db.HashSetAsync(key, field, redisValue))
            {
                await db.KeyExpireAsync(key, expiration);
            }

            return true;
        }

        /// <inheritdoc cref="ICacheFieldIncreasable.FieldIncreaseAsync{T,TField}"/>
        public async Task<TField> FieldIncreaseAsync<T, TField>(string key, string field, TField increment)
        {
            var incrementLong = Convert.ToInt64(increment);
            var db = _redis.GetDatabase(_databaseNumber);
            var tran = db.CreateTransaction();
            tran.AddCondition(Condition.HashExists(key, field));

            var incrTask = tran.HashIncrementAsync(key, field, incrementLong);
            var tranSucc = await tran.ExecuteAsync();

            if (!tranSucc)
                return default(TField);

            return (TField)Convert.ChangeType(await incrTask, typeof(TField));
        }

        /// <inheritdoc cref="ICacheFieldIncreasable.FieldIncreaseOrCreateAsync{T,TField}"/>
        public async Task<TField> FieldIncreaseOrCreateAsync<T, TField>(string key, string field, TField increment, TimeSpan expiration)
        {
            var incrementLong = Convert.ToInt64(increment);
            var db = _redis.GetDatabase(_databaseNumber);
            var res = await db.HashIncrementAsync(key, field, incrementLong);

            // 超时的处理采用和RedisCacheProvider.IncreaseOrCreate相同的方式
            if (!TimeSpan.Zero.Equals(expiration) && RedisConvert.IsNewlyCreatedAfterIncreasing(res, incrementLong))
            {
                db.KeyExpire(key, expiration);
            }

            return (TField)Convert.ChangeType(res, typeof(TField));
        }

        private bool CreateOrSet<T>(string key, T value, TimeSpan expiration, bool create)
        {
            var tran = PrepareTransactionForCreateOrSet(key, value, expiration, create);
            return tran.Execute();
        }

        private async Task<bool> CreateOrSetAsync<T>(string key, T value, TimeSpan expiration, bool create)
        {
            var tran = PrepareTransactionForCreateOrSet(key, value, expiration, create);
            return await tran.ExecuteAsync();
        }

        private ITransaction PrepareTransactionForCreateOrSet<T>(string key, T value, TimeSpan expiration, bool create)
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

            return tran;
        }

        private ITransaction PrepareTransationForFieldSet<TField>(string key, string field, TField value)
        {
            var redisValue = RedisConvert.ToRedisValue(value);
            var db = _redis.GetDatabase(_databaseNumber);

            var tran = db.CreateTransaction();
            tran.AddCondition(Condition.HashExists(key, field));
            tran.HashSetAsync(key, field, redisValue);

            return tran;
        }
    }
}
