using System;

namespace cmstar.Caching
{
    /// <summary>
    /// 包含对缓存提供器上的一个特定的缓存键的操作。
    /// </summary>
    public class KeyOperation<TValue>
    {
        private readonly CacheOperationBase _cacheManager;
        private readonly string _key;

        internal KeyOperation(CacheOperationBase cacheManager, string key)
        {
            _cacheManager = cacheManager;
            _key = key;
        }

        /// <summary>
        /// 获取当前使用的缓存键。
        /// </summary>
        public string Key
        {
            get { return _key; }
        }

        /// <summary>
        /// 获取缓存值。
        /// </summary>
        /// <returns>缓存的值。若缓存不存在，返回null。</returns>
        /// <remarks>
        /// 对于缓存值也可以为null的情况，使用此方法不能分辨缓存是否存在。
        /// 此时可使用<see cref="TryGet"/>方法替代。
        /// </remarks>
        public TValue Get()
        {
            return _cacheManager.CacheProvider.Get<TValue>(_key);
        }

        /// <summary>
        /// 获取缓存值。
        /// </summary>
        /// <param name="value">当缓存存在时，存放缓存的值。</param>
        /// <returns>true若缓存存在；否则为false。</returns>
        public bool TryGet(out TValue value)
        {
            return _cacheManager.CacheProvider.TryGet(_key, out value);
        }

        /// <summary>
        /// 设置一个缓存。
        /// </summary>
        /// <param name="value">缓存的值。</param>
        public void Set(TValue value)
        {
            var expirationSeconds = _cacheManager.Expiration.NewExpirationSeconds();
            _cacheManager.CacheProvider.Set(_key, value, TimeSpan.FromSeconds(expirationSeconds));
        }

        /// <summary>
        /// 移除具有指定键的缓存。
        /// </summary>
        public void Remove()
        {
            _cacheManager.CacheProvider.Remove(_key);
        }
    }
}