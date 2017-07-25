using System;

namespace cmstar.Caching
{
    /// <summary>
    /// 将<see cref="ICacheProviderAsync"/>适配为为<see cref="ICacheProvider"/>以便测试。
    /// </summary>
    public class CacheProviderAdapter : ICacheProvider
    {
        protected readonly ICacheProviderAsync Cache;

        public CacheProviderAdapter(ICacheProviderAsync cache)
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
    }

    /// <summary>
    /// 将<see cref="ICacheIncreasableAsync"/>适配为<see cref="ICacheIncreasable"/>以便测试。
    /// </summary>
    public class CacheIncreasableAdapter : CacheProviderAdapter, ICacheIncreasable
    {
        private readonly ICacheIncreasableAsync _cache;

        public CacheIncreasableAdapter(ICacheIncreasableAsync cache)
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
    }

    /// <summary>
    /// 将<see cref="ICacheFieldAccessableAsync"/>适配为<see cref="ICacheFieldAccessable"/>以便测试。
    /// </summary>
    public class CacheFieldAccessableAdapter : CacheProviderAdapter, ICacheFieldAccessable
    {
        private readonly ICacheFieldAccessableAsync _cache;

        public CacheFieldAccessableAdapter(ICacheFieldAccessableAsync cache)
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
    }

    /// <summary>
    /// 将<see cref="ICacheFieldIncreasableAsync"/>适配为<see cref="ICacheFieldIncreasable"/>以便测试。
    /// </summary>
    public class CacheFieldIncreasableAdapter : CacheFieldAccessableAdapter, ICacheFieldIncreasable
    {
        private readonly ICacheFieldIncreasableAsync _cache;

        public CacheFieldIncreasableAdapter(ICacheFieldIncreasableAsync cache)
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
    }
}
