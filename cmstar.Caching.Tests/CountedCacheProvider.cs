using System;
using System.Threading;
using System.Threading.Tasks;

namespace cmstar.Caching
{
    /// <summary>
    /// 记录每个缓存操作的次数。
    /// </summary>
    public class CountedCacheProvider : ICacheFieldIncreasable, ICacheIncreasable
    {
        private readonly ICacheProvider _cache;

        public CountedCacheProvider(ICacheProvider underlyingCache)
        {
            _cache = underlyingCache;
        }

        public int GetCount;
        public int TryGetCount;
        public int SetCount;
        public int CreateCount;
        public int RemoveCount;

        public int FieldGetCount;
        public int FieldTryGetCount;
        public int FieldSetCount;
        public int FieldSetOrCreateCount;

        public int IncreaseCount;
        public int IncreaseOrCreateCount;

        public int FieldIncreaseCount;
        public int FieldIncreaseOrCreateCount;

        public Task<T> GetAsync<T>(string key)
        {
            Interlocked.Increment(ref GetCount);
            return _cache.GetAsync<T>(key);
        }

        public Task<TryGetResult<T>> TryGetAsync<T>(string key)
        {
            Interlocked.Increment(ref TryGetCount);
            return _cache.TryGetAsync<T>(key);
        }

        public Task<bool> CreateAsync<T>(string key, T value, TimeSpan expiration)
        {
            Interlocked.Increment(ref CreateCount);
            return _cache.CreateAsync(key, value, expiration);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            Interlocked.Increment(ref SetCount);
            return _cache.SetAsync(key, value, expiration);
        }

        public Task<bool> RemoveAsync(string key)
        {
            Interlocked.Increment(ref RemoveCount);
            return _cache.RemoveAsync(key);
        }

        public T Get<T>(string key)
        {
            Interlocked.Increment(ref GetCount);
            return _cache.Get<T>(key);
        }

        public bool TryGet<T>(string key, out T value)
        {
            Interlocked.Increment(ref TryGetCount);
            return _cache.TryGet(key, out value);
        }

        public bool Create<T>(string key, T value, TimeSpan expiration)
        {
            Interlocked.Increment(ref CreateCount);
            return _cache.Create(key, value, expiration);
        }

        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            Interlocked.Increment(ref SetCount);
            _cache.Set(key, value, expiration);
        }

        public bool Remove(string key)
        {
            Interlocked.Increment(ref RemoveCount);
            return _cache.Remove(key);
        }

        public Task<TField> FieldGetAsync<T, TField>(string key, string field)
        {
            Interlocked.Increment(ref FieldGetCount);
            return GetCacheFieldAccessableProvider().FieldGetAsync<T, TField>(key, field);
        }

        public Task<TryGetResult<TField>> FieldTryGetAsync<T, TField>(string key, string field)
        {
            Interlocked.Increment(ref FieldTryGetCount);
            return GetCacheFieldAccessableProvider().FieldTryGetAsync<T, TField>(key, field);
        }

        public Task<bool> FieldSetAsync<T, TField>(string key, string field, TField value)
        {
            Interlocked.Increment(ref FieldSetCount);
            return GetCacheFieldAccessableProvider().FieldSetAsync<T, TField>(key, field, value);
        }

        public Task<bool> FieldSetOrCreateAsync<T, TField>(string key, string field, TField value, TimeSpan expiration)
        {
            Interlocked.Increment(ref FieldSetOrCreateCount);
            return GetCacheFieldAccessableProvider().FieldSetOrCreateAsync<T, TField>(key, field, value, expiration);
        }

        public TField FieldGet<T, TField>(string key, string field)
        {
            Interlocked.Increment(ref FieldGetCount);
            return GetCacheFieldAccessableProvider().FieldGet<T, TField>(key, field);
        }

        public bool FieldTryGet<T, TField>(string key, string field, out TField value)
        {
            Interlocked.Increment(ref FieldTryGetCount);
            return GetCacheFieldAccessableProvider().FieldTryGet<T, TField>(key, field, out value);
        }

        public bool FieldSet<T, TField>(string key, string field, TField value)
        {
            Interlocked.Increment(ref FieldSetCount);
            return GetCacheFieldAccessableProvider().FieldSet<T, TField>(key, field, value);
        }

        public bool FieldSetOrCreate<T, TField>(string key, string field, TField value, TimeSpan expiration)
        {
            Interlocked.Increment(ref FieldSetOrCreateCount);
            return GetCacheFieldAccessableProvider().FieldSetOrCreate<T, TField>(key, field, value, expiration);
        }

        public Task<TField> FieldIncreaseAsync<T, TField>(string key, string field, TField increment)
        {
            Interlocked.Increment(ref FieldIncreaseCount);
            return GetCacheFieldIncreasableProvider().FieldIncreaseAsync<T, TField>(key, field, increment);
        }

        public Task<TField> FieldIncreaseOrCreateAsync<T, TField>(string key, string field, TField increment, TimeSpan expiration)
        {
            Interlocked.Increment(ref FieldIncreaseOrCreateCount);
            return GetCacheFieldIncreasableProvider().FieldIncreaseOrCreateAsync<T, TField>(key, field, increment, expiration);
        }

        public TField FieldIncrease<T, TField>(string key, string field, TField increment)
        {
            Interlocked.Increment(ref FieldIncreaseCount);
            return GetCacheFieldIncreasableProvider().FieldIncrease<T, TField>(key, field, increment);
        }

        public TField FieldIncreaseOrCreate<T, TField>(string key, string field, TField increment, TimeSpan expiration)
        {
            Interlocked.Increment(ref FieldIncreaseOrCreateCount);
            return GetCacheFieldIncreasableProvider().FieldIncreaseOrCreate<T, TField>(key, field, increment, expiration);
        }

        public Task<T> IncreaseAsync<T>(string key, T increment)
        {
            Interlocked.Increment(ref IncreaseCount);
            return GetCacheIncreasableProvider().IncreaseAsync(key, increment);
        }

        public Task<T> IncreaseOrCreateAsync<T>(string key, T increment, TimeSpan expiration)
        {
            Interlocked.Increment(ref IncreaseOrCreateCount);
            return GetCacheIncreasableProvider().IncreaseOrCreateAsync(key, increment, expiration);
        }

        public T Increase<T>(string key, T increment)
        {
            Interlocked.Increment(ref IncreaseCount);
            return GetCacheIncreasableProvider().Increase(key, increment);
        }

        public T IncreaseOrCreate<T>(string key, T increment, TimeSpan expiration)
        {
            Interlocked.Increment(ref IncreaseOrCreateCount);
            return GetCacheIncreasableProvider().IncreaseOrCreate(key, increment, expiration);
        }

        private ICacheIncreasable GetCacheIncreasableProvider()
        {
            var res = _cache as ICacheIncreasable;
            if (res == null)
                throw InvalidOperationError();

            return res;
        }

        private ICacheFieldAccessable GetCacheFieldAccessableProvider()
        {
            var res = _cache as ICacheFieldAccessable;
            if (res == null)
                throw InvalidOperationError();

            return res;
        }

        private ICacheFieldIncreasable GetCacheFieldIncreasableProvider()
        {
            var res = _cache as ICacheFieldIncreasable;
            if (res == null)
                throw InvalidOperationError();

            return res;
        }

        private InvalidOperationException InvalidOperationError()
        {
            var msg = $"The operation is not valid on the cache provider {_cache.GetType()}.";
            return new InvalidOperationException(msg);
        }
    }
}
