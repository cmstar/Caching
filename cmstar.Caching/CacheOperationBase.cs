namespace cmstar.Caching
{
    /// <summary>
    /// 包含缓存的操作的封装。这是一个抽象类。
    /// </summary>
    public abstract class CacheOperationBase
    {
        protected readonly string CacheNamespace;
        protected readonly string KeyBase;

        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="cacheNamespace">指定缓存键所在的命名空间。</param>
        /// <param name="keyRoot">指定用于构建缓存键的前缀。</param>
        /// <param name="provider">缓存提供器。</param>
        /// <param name="expiration">设定缓存的超时时间。</param>
        protected CacheOperationBase(
            string cacheNamespace, string keyRoot, ICacheProvider provider, CacheExpiration expiration)
        {
            ArgAssert.NotNull(provider, "provider");
            ArgAssert.NotNull(expiration, "expiration");
            ArgAssert.NotNullOrEmpty(keyRoot, "keyRoot");

            CacheProvider = provider;
            Expiration = expiration;
            CacheNamespace = cacheNamespace;

            KeyBase = string.Concat(CacheNamespace, ":", keyRoot);
        }

        /// <summary>
        /// 获取当前实例绑定的缓存过期设置。
        /// </summary>
        public CacheExpiration Expiration { get; private set; }

        /// <summary>
        /// 获取当前实例所使用的缓存提供器。
        /// </summary>
        public ICacheProvider CacheProvider { get; private set; }
    }
}
