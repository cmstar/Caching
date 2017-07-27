using System;
using System.Diagnostics;

namespace cmstar.Caching
{
    /// <summary>
    /// 实现简单的两级缓存。
    /// </summary>
    /// <remarks>
    /// 注意：两个不同层次的缓存将使用相同的缓存键。
    /// 两级缓存需由不同的缓存提供器提供，使用诸如<see cref="HttpRuntimeCacheProvider"/>
    /// 等底层实现为单例的缓存提供器，将导致一级和二级缓存间互相覆盖从而导致预期外的结果。
    /// </remarks>
    public partial class L2CacheProvider : ICacheProvider
    {
        private readonly CacheExpiration _level1Expiration;
        private readonly ICacheProvider _level1;
        private readonly ICacheProvider _level2;

        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="level2">指定作为第二级缓存的缓存提供器。</param>
        /// <param name="level1">
        /// 指定作为第一级缓存的缓存提供器。一级缓存通常快于二级缓存的，并且回收间隔更短。
        /// </param>
        /// <param name="level1Expiration">指定一级缓存的内置超时时间。</param>
        public L2CacheProvider(ICacheProvider level2, ICacheProvider level1, CacheExpiration level1Expiration)
        {
            ArgAssert.NotNull(level2, nameof(level2));
            ArgAssert.NotNull(level1, nameof(level1));
            ArgAssert.NotNull(level1Expiration, nameof(level1Expiration));

            _level1Expiration = level1Expiration;
            _level1 = level1;
            _level2 = level2;
        }

        /// <inheritdoc cref="ICacheProvider.Get{T}"/>
        public T Get<T>(string key)
        {
            T value;
            return TryGet(key, out value) ? value : default(T);
        }

        /// <inheritdoc cref="ICacheProvider.TryGet{T}"/>
        public bool TryGet<T>(string key, out T value)
        {
            if (_level1.TryGet(key, out value))
            {
                Log($"Cache value of key {key} retrieved from level 1.");
                return true;
            }

            if (_level2.TryGet(key, out value))
            {
                SetLevel1(key, value);

                Log($"Cache value of key {key} retrieved from level 2.");
                return true;
            }

            return false;
        }

        /// <inheritdoc cref="ICacheProvider.Set{T}"/>
        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            SetLevel1(key, value);
            Log($"Set into level1 with key {key}");

            _level2.Set(key, value, expiration);
            Log($"Set into level2 with key {key}");
        }

        /// <inheritdoc cref="ICacheProvider.Create{T}"/>
        public bool Create<T>(string key, T value, TimeSpan expiration)
        {
            if (!_level2.Create(key, value, expiration))
                return false;

            SetLevel1(key, value);
            return true;
        }

        /// <inheritdoc cref="ICacheProvider.Remove"/>
        public bool Remove(string key)
        {
            var res2 = _level2.Remove(key);
            Log($"Remove from level2: {res2}");

            var res1 = _level1.Remove(key);
            Log($"Remove from level1: {res1}");

            return res2;
        }

        private void SetLevel1<T>(string key, T value)
        {
            var level1ExpirationSeconds = _level1Expiration.NewExpirationSeconds();
            _level1.Set(key, value, TimeSpan.FromSeconds(level1ExpirationSeconds));
        }

        [Conditional("DEBUG")]
        private void Log(string msg)
        {
            Trace.WriteLine($"[{GetType().Name}] {msg}");
        }
    }
}
