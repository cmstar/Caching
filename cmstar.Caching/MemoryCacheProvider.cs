using System;
using System.Runtime.Caching;

namespace cmstar.Caching
{
    /// <summary>
    /// 基于<see cref="MemoryCache"/>的缓存提供器实现。
    /// </summary>
    public class MemoryCacheProvider : ICacheProvider
    {
        private readonly MemoryCache _cache;

        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="name">对应<see cref="MemoryCache.Name"/>。</param>
        public MemoryCacheProvider(string name)
        {
            _cache = new MemoryCache(name);
        }

        public T Get<T>(string key)
        {
            var v = _cache.Get(key);
            return ReferenceEquals(CacheUtils.NullValue, v) ? default(T) : (T)v;
        }

        public bool TryGet<T>(string key, out T value)
        {
            var v = _cache.Get(key);

            if (v == null || ReferenceEquals(CacheUtils.NullValue, v))
            {
                value = default(T);
                return false;
            }

            value = (T)v;
            return true;
        }

        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            var e = TimeSpan.Zero.Equals(expiration)
                ? ObjectCache.InfiniteAbsoluteExpiration
                : DateTimeOffset.Now.Add(expiration);

            object v = value;
            _cache.Set(key, v ?? CacheUtils.NullValue, e);
        }

        public bool Remove(string key)
        {
            return _cache.Remove(key) != null;
        }
    }
}
