namespace cmstar.Caching
{
    /// <summary>
    /// 包含缓存的操作的封装。这是一个抽象类。
    /// </summary>
    public abstract class CacheOperationBase
    {
        /// <summary>
        /// 缓存键的命名空间，用于缓存空间的管理（目前并没有实现）。
        /// </summary>
        public readonly string CacheNamespace;

        /// <summary>
        /// 缓存键的完整前缀，以命名空间开头，格式如 NAMESPACE:KEYROOT。
        /// </summary>
        public readonly string KeyBase;

        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="cacheNamespace">指定缓存键所在的命名空间。</param>
        /// <param name="keyRoot">指定用于构建缓存键的前缀。</param>
        /// <param name="provider">缓存提供器。</param>
        /// <param name="expiration">设定缓存的超时时间。</param>
        /// <param name="observer">
        /// 指定用于当前实例的<see cref="ICacheOperationObserver"/>，若不需要绑定
        /// <see cref="ICacheOperationObserver"/>，使用<c>null</c>。
        /// </param>
        protected CacheOperationBase(
            string cacheNamespace,
            string keyRoot,
            ICacheProvider provider,
            CacheExpiration expiration,
            ICacheOperationObserver observer)
        {
            ArgAssert.NotNull(provider, nameof(provider));
            ArgAssert.NotNull(expiration, nameof(expiration));
            ArgAssert.NotNullOrEmpty(keyRoot, nameof(keyRoot));

            CacheProvider = provider;
            Expiration = expiration;
            CacheNamespace = cacheNamespace;
            Observer = observer;

            KeyBase = string.Concat(CacheNamespace, ":", keyRoot);
        }

        /// <summary>
        /// 获取当前实例绑定的缓存过期设置。
        /// </summary>
        public CacheExpiration Expiration { get; }

        /// <summary>
        /// 获取当前实例所使用的缓存提供器。
        /// </summary>
        public ICacheProvider CacheProvider { get; }

        /// <summary>
        /// 获取或设置当前实例所绑定的<see cref="ICacheOperationObserver"/>，若没有绑定，值为<c>null</c>。
        /// </summary>
        public ICacheOperationObserver Observer { get; set; }

        /// <summary>
        /// 当前使用的缓存提供器是否支持缓存值的增减操作。
        /// 若为true，则<see cref="CacheProvider"/>实现了<see cref="ICacheIncreasable"/>。
        /// </summary>
        public bool CanIncrease => CacheProvider is ICacheIncreasable;

        /// <summary>
        /// 当前使用的缓存提供器是否支持缓存对象的字段操作。
        /// 若为true，则<see cref="CacheProvider"/>实现了<see cref="ICacheFieldAccessable"/>。
        /// </summary>
        public bool CanAccessField => CacheProvider is ICacheFieldAccessable;

        /// <summary>
        /// 当前使用的缓存提供器是否支持缓存对象的字段值的增减操作。
        /// 若为true，则<see cref="CacheProvider"/>实现了<see cref="ICacheFieldIncreasable"/>。
        /// </summary>
        public bool CanIncreaseField => CacheProvider is ICacheFieldIncreasable;
    }
}
