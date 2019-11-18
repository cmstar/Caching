#if NETSTANDARD
using System;
using Microsoft.Extensions.Caching.Memory;

namespace cmstar.Caching
{
    /// <summary>
    /// 基于<see cref="MemoryCache"/>的缓存提供器实现。
    /// </summary>
    public class MemoryCacheProvider : MemoryBasedCacheProvider
    {
        private readonly IMemoryCache _cache;

        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        public MemoryCacheProvider()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="name">指定当前实例的名称。</param>
        public MemoryCacheProvider(string name)
        {
            Name = name ?? string.Empty;

            var option = new MemoryCacheOptions();
            _cache = new MemoryCache(option);
        }

        ~MemoryCacheProvider()
        {
            _cache.Dispose();
        }

        /// <summary>
        /// 获取当前实例的名称。
        /// </summary>
        public string Name { get; }

        /// <inheritdoc />
        protected override object DoGet(string key)
        {
            return _cache.TryGetValue(key, out var value)
                ? value
                : null;
        }

        /// <inheritdoc />
        protected override void DoSet(string key, object value, TimeSpan expiration)
        {
            var e = expiration.Equals(TimeSpan.Zero)
                ? DateTimeOffset.MaxValue
                : DateTimeOffset.Now.Add(expiration);

            _cache.Set(key, value, e);
        }

        /// <inheritdoc />
        protected override bool DoCreate(string key, object value, TimeSpan expiration)
        {
            if (_cache.TryGetValue(key, out object _))
                return false;

            DoSet(key, value, expiration);
            return true;
        }

        /// <inheritdoc />
        protected override bool DoRemove(string key)
        {
            var hasValue = _cache.TryGetValue(key, out var _);
            _cache.Remove(key);
            return hasValue;
        }
    }
}
#endif