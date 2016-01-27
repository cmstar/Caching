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

        protected override object DoGet(string key)
        {
            var v = _cache.Get(key);
            return v;
        }

        protected override void DoSet(string key, object value, TimeSpan expiration)
        {
            var e = TimeSpan.Zero.Equals(expiration)
                ? ObjectCache.InfiniteAbsoluteExpiration
                : DateTimeOffset.Now.Add(expiration);

            _cache.Set(key, value, e);
        }

        protected override bool DoRemove(string key)
        {
            return _cache.Remove(key) != null;
        }
    }
}