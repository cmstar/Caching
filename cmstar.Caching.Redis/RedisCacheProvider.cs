using System;
using System.Threading.Tasks;
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

        /// <summary>
        /// 获取或设置一个值，该值表示实例内的 *Async 方法是否使用异步执行。
        /// 当值为<c>true</c>时，实例内的 *Async 方法使用异步执行；否则使用非异步方式执行。
        /// 默认值为<c>true</c>。
        /// </summary>
        public bool AsyncEnabled { get; set; } = true;

        /// <inheritdoc cref="ICacheProvider.Get{T}" />
        public T Get<T>(string key)
        {
            TryGet(key, out T value);
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

            value = RedisConvert.FromRedisValue<T>(redisValue);
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

            try
            {
                var res = tran.StringIncrementAsync(key, incrementLong);
                return tran.Execute()
                    ? (T)Convert.ChangeType(res.Result, typeof(T))
                    : default(T);
            }
            catch (Exception ex)
            {
                var convertedException = RedisConvert.TryConvertExceptionForNumberIncrementOnInvalidValue(ex);
                if (convertedException != null)
                    throw convertedException;

                throw;
            }
        }

        /// <inheritdoc cref="ICacheIncreasable.IncreaseOrCreate{T}" />
        public T IncreaseOrCreate<T>(string key, T increment, TimeSpan expiration)
        {
            var incrementLong = Convert.ToInt64(increment);
            var db = _redis.GetDatabase(_databaseNumber);

            long res;
            try
            {
                res = db.StringIncrement(key, incrementLong);
            }
            catch (Exception ex)
            {
                var convertedException = RedisConvert.TryConvertExceptionForNumberIncrementOnInvalidValue(ex);
                if (convertedException != null)
                    throw convertedException;

                throw;
            }

            // 如果值是新建的，需要设置其超时时间。
            if (!TimeSpan.Zero.Equals(expiration) && RedisConvert.IsNewlyCreatedAfterIncreasing(res, incrementLong))
            {
                db.KeyExpire(key, expiration);
            }

            return (T)Convert.ChangeType(res, typeof(T));
        }

        /// <inheritdoc cref="ICacheProvider.GetAsync{T}" />
        public async Task<T> GetAsync<T>(string key)
        {
            if (!AsyncEnabled)
                return Get<T>(key);

            var res = await TryGetAsync<T>(key);
            return res.Value;
        }

        /// <inheritdoc cref="ICacheProvider.TryGetAsync{T}" />
        public async Task<TryGetResult<T>> TryGetAsync<T>(string key)
        {
            if (!AsyncEnabled)
                return new TryGetResult<T>(TryGet<T>(key, out var result), result);

            var db = _redis.GetDatabase(_databaseNumber);
            var redisValue = await db.StringGetAsync(key);
            var hasValue = !redisValue.IsNull;
            var value = hasValue
                ? RedisConvert.FromRedisValue<T>(redisValue)
                : default(T);

            return new TryGetResult<T>(hasValue, value);
        }

        /// <inheritdoc cref="ICacheProvider.SetAsync{T}" />
        public Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            return InternalSetAsync(key, value, expiration, When.Always);
        }

        /// <inheritdoc cref="ICacheProvider.CreateAsync{T}" />
        public Task<bool> CreateAsync<T>(string key, T value, TimeSpan expiration)
        {
            return InternalSetAsync(key, value, expiration, When.NotExists);
        }

        /// <inheritdoc cref="ICacheProvider.RemoveAsync" />
        public Task<bool> RemoveAsync(string key)
        {
            if (!AsyncEnabled)
                return Task.FromResult(Remove(key));

            var db = _redis.GetDatabase(_databaseNumber);
            return db.KeyDeleteAsync(key);
        }

        /// <inheritdoc cref="ICacheIncreasable.IncreaseAsync{T}" />
        public async Task<T> IncreaseAsync<T>(string key, T increment)
        {
            if (!AsyncEnabled)
                return Increase(key, increment);

            var incrementLong = Convert.ToInt64(increment);
            var db = _redis.GetDatabase(_databaseNumber);

            var tran = db.CreateTransaction();
            tran.AddCondition(Condition.KeyExists(key));

            try
            {
                var incrTask = tran.StringIncrementAsync(key, incrementLong);
                var tranSucc = await tran.ExecuteAsync();
                if (!tranSucc)
                    return default(T);

                // 根据SE.Redis的文档，await ExecuteAsync 之后，事务内的操作肯定都完成了，
                // 所以这里其实是可以访问 incrTask.Result 的。但我们应当尽量避免在 async
                // 代码内这样做，所以这里仍然 await 之。
                return (T)Convert.ChangeType(await incrTask, typeof(T));
            }
            catch (Exception ex)
            {
                var convertedException = RedisConvert.TryConvertExceptionForNumberIncrementOnInvalidValue(ex);
                if (convertedException != null)
                    throw convertedException;

                throw;
            }
        }

        /// <inheritdoc cref="ICacheIncreasable.IncreaseOrCreateAsync{T}" />
        public async Task<T> IncreaseOrCreateAsync<T>(string key, T increment, TimeSpan expiration)
        {
            if (!AsyncEnabled)
                return IncreaseOrCreate(key, increment, expiration);

            var incrementLong = Convert.ToInt64(increment);
            var db = _redis.GetDatabase(_databaseNumber);

            long res;
            try
            {
                res = await db.StringIncrementAsync(key, incrementLong);
            }
            catch (Exception ex)
            {
                var convertedException = RedisConvert.TryConvertExceptionForNumberIncrementOnInvalidValue(ex);
                if (convertedException != null)
                    throw convertedException;

                throw;
            }

            // 如果值是新建的，需要设置其超时时间。
            if (!TimeSpan.Zero.Equals(expiration) && RedisConvert.IsNewlyCreatedAfterIncreasing(res, incrementLong))
            {
                await db.KeyExpireAsync(key, expiration);
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

        private Task<bool> InternalSetAsync<T>(string key, T value, TimeSpan expiration, When when)
        {
            if (!AsyncEnabled)
                return Task.FromResult(InternalSet(key, value, expiration, when));

            var e = TimeSpan.Zero.Equals(expiration) ? (TimeSpan?)null : expiration;
            var db = _redis.GetDatabase(_databaseNumber);
            var redisValue = RedisConvert.ToRedisValue(value);
            return db.StringSetAsync(key, redisValue, e, when);
        }
    }
}
