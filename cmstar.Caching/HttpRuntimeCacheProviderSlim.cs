using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using CacheItemPriority = System.Web.Caching.CacheItemPriority;

namespace cmstar.Caching
{
    /// <summary>
    /// 基于<see cref="HttpRuntime.Cache"/>的缓存提供器实现。
    /// 这是<see cref="HttpRuntimeCacheProvider"/>的简化版本，仅实现基本的缓存功能，但具有更高的性能，
    /// 适用于仅使用了基本缓存功能且需要高性能的场景。
    /// </summary>
    public class HttpRuntimeCacheProviderSlim : ICacheProvider
    {
        /// <summary>
        /// 获取<see cref="HttpRuntimeCacheProviderSlim"/>的唯一实例。
        /// </summary>
        public static readonly HttpRuntimeCacheProviderSlim Instance = new HttpRuntimeCacheProviderSlim();

        private HttpRuntimeCacheProviderSlim() { }

        /// <inheritdoc />
        public T Get<T>(string key)
        {
            var cacheValue = HttpRuntime.Cache.Get(key);
            if (cacheValue == null || ReferenceEquals(CacheEnv.NullValue, cacheValue))
                return default(T);

            var rawValue = ((CacheValue)cacheValue).Value;
            return rawValue is T
                ? (T)rawValue
                : (T)Convert.ChangeType(rawValue, typeof(T));
        }

        /// <inheritdoc />
        public bool TryGet<T>(string key, out T value)
        {
            var cacheValue = HttpRuntime.Cache.Get(key);
            if (cacheValue == null)
            {
                value = default(T);
                return false;
            }

            if (ReferenceEquals(CacheEnv.NullValue, cacheValue))
            {
                value = default(T);
                return true;
            }

            var rawValue = ((CacheValue)cacheValue).Value;
            value = rawValue is T
                ? (T)rawValue
                : (T)Convert.ChangeType(rawValue, typeof(T));

            return true;
        }

        /// <inheritdoc />
        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            var cacheValue = value == null ? CacheEnv.NullValue : new CacheValue(value);
            var e = TimeSpan.Zero.Equals(expiration)
                ? DateTime.MaxValue
                : DateTime.UtcNow.Add(expiration);

            HttpRuntime.Cache.Insert(key, cacheValue, null, e, Cache.NoSlidingExpiration);
        }

        /// <inheritdoc />
        public bool Create<T>(string key, T value, TimeSpan expiration)
        {
            var cacheValue = value == null ? CacheEnv.NullValue : new CacheValue(value);
            var e = TimeSpan.Zero.Equals(expiration)
                ? DateTime.MaxValue
                : DateTime.UtcNow.Add(expiration);

            var oldValue = HttpRuntime.Cache.Add(
                key, cacheValue, null, e, Cache.NoSlidingExpiration, CacheItemPriority.Default, null);

            return oldValue == null;
        }

        /// <inheritdoc />
        public bool Remove(string key)
        {
            return HttpRuntime.Cache.Remove(key) != null;
        }

        /// <inheritdoc />
        public Task<T> GetAsync<T>(string key)
        {
            var res = Get<T>(key);
            return Task.FromResult(res);
        }

        /// <inheritdoc />
        public Task<TryGetResult<T>> TryGetAsync<T>(string key)
        {
            T value;
            var hasValue = TryGet(key, out value);
            var res = new TryGetResult<T>(hasValue, value);
            return Task.FromResult(res);
        }

        /// <inheritdoc />
        public Task<bool> CreateAsync<T>(string key, T value, TimeSpan expiration)
        {
            var res = Create(key, value, expiration);
            return Task.FromResult(res);
        }

        /// <inheritdoc />
        public Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            Set(key, value, expiration);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<bool> RemoveAsync(string key)
        {
            var res = Remove(key);
            return Task.FromResult(res);
        }
    }
}
