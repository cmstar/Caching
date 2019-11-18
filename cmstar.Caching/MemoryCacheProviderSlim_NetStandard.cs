#if NETSTANDARD
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace cmstar.Caching
{
    /// <summary>
    /// 基于<see cref="MemoryCache"/>的缓存提供器实现。
    /// 这是<see cref="MemoryCacheProvider"/>的简化版本，仅实现基本的缓存功能，但具有更高的性能，
    /// 适用于仅使用了基本缓存功能且需要高性能的场景。
    /// </summary>
    public class MemoryCacheProviderSlim : ICacheProvider
    {
        private readonly IMemoryCache _cache;

        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        public MemoryCacheProviderSlim()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="name">指定当前实例的名称。</param>
        public MemoryCacheProviderSlim(string name)
        {
            Name = name ?? string.Empty;

            var option = new MemoryCacheOptions();
            _cache = new MemoryCache(option);
        }

        ~MemoryCacheProviderSlim()
        {
            _cache.Dispose();
        }

        /// <summary>
        /// 获取当前实例的名称。
        /// </summary>
        public string Name { get; }

        /// <inheritdoc />
        public T Get<T>(string key)
        {
            return _cache.TryGetValue(key, out T value)
                ? value
                : default(T);
        }

        /// <inheritdoc />
        public bool TryGet<T>(string key, out T value)
        {
            return _cache.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            var cacheValue = value == null ? CacheEnv.NullValue : new CacheValue(value);
            var e = expiration.Equals(TimeSpan.Zero)
                ? DateTimeOffset.MaxValue
                : DateTimeOffset.Now.Add(expiration);

            _cache.Set(key, cacheValue, e);
        }

        /// <inheritdoc />
        public bool Create<T>(string key, T value, TimeSpan expiration)
        {
            var hasValue = _cache.TryGetValue(key, out var _);
            if (hasValue)
                return false;

            Set(key, value, expiration);
            return true;
        }

        /// <inheritdoc />
        public bool Remove(string key)
        {
            var hasValue = _cache.TryGetValue(key, out var _);
            _cache.Remove(key);
            return hasValue;
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
            var hasValue = TryGet(key, out T value);
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
#endif