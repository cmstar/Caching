using System;
using System.Threading.Tasks;

namespace cmstar.Caching
{
    /*
     * 将缓存提供器的异步操作转为非异步操作，使非异步操作的测试用例可以用在异步操作上。
     */

    /// <summary>
    /// 将<see cref="ICacheProvider"/>中的异步操作转换为对应的非异步操作。
    /// </summary>
    public class CacheProviderAdapter : ICacheProvider
    {
        protected readonly ICacheProvider Cache;

        public CacheProviderAdapter(ICacheProvider cache)
        {
            Cache = cache;
        }

        public T Get<T>(string key)
        {
            return Cache.GetAsync<T>(key).Result;
        }

        public bool TryGet<T>(string key, out T value)
        {
            var res = Cache.TryGetAsync<T>(key).Result;
            value = res.Value;
            return res.HasValue;
        }

        public bool Create<T>(string key, T value, TimeSpan expiration)
        {
            return Cache.CreateAsync(key, value, expiration).Result;
        }

        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            Cache.SetAsync(key, value, expiration).Wait();
        }

        public bool Remove(string key)
        {
            return Cache.RemoveAsync(key).Result;
        }

        public Task<T> GetAsync<T>(string key)
        {
            throw new NotSupportedException();
        }

        public Task<TryGetResult<T>> TryGetAsync<T>(string key)
        {
            throw new NotSupportedException();
        }

        public Task<bool> CreateAsync<T>(string key, T value, TimeSpan expiration)
        {
            throw new NotSupportedException();
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            throw new NotSupportedException();
        }

        public Task<bool> RemoveAsync(string key)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// 将<see cref="ICacheIncreasable"/>中的异步操作转换为对应的非异步操作。
    /// </summary>
    public class CacheIncreasableAdapter : CacheProviderAdapter, ICacheIncreasable
    {
        private readonly ICacheIncreasable _cache;

        public CacheIncreasableAdapter(ICacheIncreasable cache)
            : base(cache)
        {
            _cache = cache;
        }

        public T Increase<T>(string key, T increment)
        {
            return _cache.IncreaseAsync(key, increment).Result;
        }

        public T IncreaseOrCreate<T>(string key, T increment, TimeSpan expiration)
        {
            return _cache.IncreaseOrCreateAsync(key, increment, expiration).Result;
        }

        public Task<T> IncreaseAsync<T>(string key, T increment)
        {
            throw new NotSupportedException();
        }

        public Task<T> IncreaseOrCreateAsync<T>(string key, T increment, TimeSpan expiration)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// 将<see cref="ICacheFieldAccessable"/>中的异步操作转换为对应的非异步操作。
    /// </summary>
    public class CacheFieldAccessableAdapter : CacheProviderAdapter, ICacheFieldAccessable
    {
        private readonly ICacheFieldAccessable _cache;

        public CacheFieldAccessableAdapter(ICacheFieldAccessable cache)
            : base(cache)
        {
            _cache = cache;
        }

        public TField FieldGet<T, TField>(string key, string field)
        {
            return _cache.FieldGetAsync<T, TField>(key, field).Result;
        }

        public bool FieldSet<T, TField>(string key, string field, TField value)
        {
            return _cache.FieldSetAsync<T, TField>(key, field, value).Result;
        }

        public bool FieldSetOrCreate<T, TField>(string key, string field, TField value, TimeSpan expiration)
        {
            return _cache.FieldSetOrCreateAsync<T, TField>(key, field, value, expiration).Result;
        }

        public bool FieldTryGet<T, TField>(string key, string field, out TField value)
        {
            var res = _cache.FieldTryGetAsync<T, TField>(key, field).Result;
            value = res.Value;
            return res.HasValue;
        }

        public Task<TField> FieldGetAsync<T, TField>(string key, string field)
        {
            throw new NotSupportedException();
        }

        public Task<TryGetResult<TField>> FieldTryGetAsync<T, TField>(string key, string field)
        {
            throw new NotSupportedException();
        }

        public Task<bool> FieldSetAsync<T, TField>(string key, string field, TField value)
        {
            throw new NotSupportedException();
        }

        public Task<bool> FieldSetOrCreateAsync<T, TField>(string key, string field, TField value, TimeSpan expiration)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// 将<see cref="ICacheFieldIncreasable"/>中的异步操作转换为对应的非异步操作。
    /// </summary>
    public class CacheFieldIncreasableAdapter : CacheFieldAccessableAdapter, ICacheFieldIncreasable
    {
        private readonly ICacheFieldIncreasable _cache;

        public CacheFieldIncreasableAdapter(ICacheFieldIncreasable cache)
            : base(cache)
        {
            _cache = cache;
        }

        public TField FieldIncrease<T, TField>(string key, string field, TField increment)
        {
            return _cache.FieldIncreaseAsync<T, TField>(key, field, increment).Result;
        }

        public TField FieldIncreaseOrCreate<T, TField>(string key, string field, TField increment, TimeSpan expiration)
        {
            return _cache.FieldIncreaseOrCreateAsync<T, TField>(key, field, increment, expiration).Result;
        }

        public Task<TField> FieldIncreaseAsync<T, TField>(string key, string field, TField increment)
        {
            throw new NotSupportedException();
        }

        public Task<TField> FieldIncreaseOrCreateAsync<T, TField>(string key, string field, TField increment, TimeSpan expiration)
        {
            throw new NotSupportedException();
        }
    }
}
