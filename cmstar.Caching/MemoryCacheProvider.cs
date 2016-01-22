using System;
using System.Runtime.Caching;

namespace cmstar.Caching
{
    /// <summary>
    /// 基于<see cref="MemoryCache"/>的缓存提供器实现。
    /// </summary>
    public class MemoryCacheProvider : MemoryBasedCacheProvider
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

        protected override T DoGet<T>(string key)
        {
            var v = _cache.Get(key);
            return v == null || ReferenceEquals(CacheEnv.NullValue, v) ? default(T) : (T)v;
        }

        protected override bool DoTryGet<T>(string key, out T value)
        {
            var v = _cache.Get(key);

            if (v == null || ReferenceEquals(CacheEnv.NullValue, v))
            {
                value = default(T);
                return v != null;
            }

            value = (T)v;
            return true;
        }

        protected override void DoSet<T>(string key, T value, TimeSpan expiration)
        {
            var e = TimeSpan.Zero.Equals(expiration)
                ? ObjectCache.InfiniteAbsoluteExpiration
                : DateTimeOffset.Now.Add(expiration);

            object v = value;
            _cache.Set(key, v ?? CacheEnv.NullValue, e);
        }

        protected override bool DoRemove(string key)
        {
            return _cache.Remove(key) != null;
        }
    }
}
