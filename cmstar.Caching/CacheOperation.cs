namespace cmstar.Caching
{
    /// <summary>
    /// 包含缓存的操作的封装，并提供强类型的缓存键值的管理。
    /// </summary>
    /// <typeparam name="TRes">缓存的值的类型。</typeparam>
    public class CacheOperation<TRes> : CacheOperationBase
    {
        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="cacheNamespace">指定缓存键所在的命名空间。</param>
        /// <param name="keyRoot">指定用于构建缓存键的前缀。</param>
        /// <param name="provider">缓存提供器。</param>
        /// <param name="expiration">设定缓存的超时时间。</param>
        public CacheOperation(
            string cacheNamespace, string keyRoot, ICacheProvider provider, CacheExpiration expiration)
            : base(cacheNamespace, keyRoot, provider, expiration)
        {
        }

        /// <summary>
        /// 获取一个用于操作给定的缓存键的对象。
        /// </summary>
        /// <returns>操作给定的缓存键的实例。</returns>
        public KeyOperation<TRes> Key()
        {
            return new KeyOperation<TRes>(this, KeyBase);
        }

        /// <summary>
        /// 获取一个用于操作给定的缓存键的对象。
        /// </summary>
        /// <param name="elements">用于构建缓存键的数据的集合。</param>
        /// <returns>用于操作给定的缓存键的对象。</returns>
        public KeyOperation<TRes> Key(params object[] elements)
        {
            var key = CacheUtils.BuildCacheKey(KeyBase, elements);
            return new KeyOperation<TRes>(this, key);
        }
    }

    /// <summary>
    /// 包含缓存的操作，并提供强类型的缓存键值的管理。
    /// </summary>
    /// <typeparam name="TRes">缓存的值的类型。</typeparam>
    /// <typeparam name="T1">用于构建缓存键的数据的类型。</typeparam>
    public class CacheOperation<T1, TRes> : CacheOperationBase
    {
        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="cacheNamespace">指定缓存键所在的命名空间。</param>
        /// <param name="keyRoot">指定用于构建缓存键的前缀。</param>
        /// <param name="provider">缓存提供器。</param>
        /// <param name="expiration">设定缓存的超时时间。</param>
        public CacheOperation(
            string cacheNamespace, string keyRoot, ICacheProvider provider, CacheExpiration expiration)
            : base(cacheNamespace, keyRoot, provider, expiration)
        {
        }

        /// <summary>
        /// 获取一个用于操作给定的缓存键的对象。
        /// </summary>
        /// <param name="v">用于构建缓存键的数据。</param>
        /// <returns>操作给定的缓存键的实例。</returns>
        public KeyOperation<TRes> Key(T1 v)
        {
            var key = CacheUtils.BuildCacheKey(KeyBase,
                ReferenceEquals(v, null) ? string.Empty : v.ToString());
            return new KeyOperation<TRes>(this, key);
        }
    }

    /// <summary>
    /// 包含缓存的操作，并提供强类型的缓存键值的管理。
    /// </summary>
    /// <typeparam name="TRes">缓存的值的类型。</typeparam>
    /// <typeparam name="T1">用于构建缓存键的第1个数据的类型。</typeparam>
    /// <typeparam name="T2">用于构建缓存键的第2个数据的类型。</typeparam>
    public class CacheOperation<T1, T2, TRes> : CacheOperationBase
    {
        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="cacheNamespace">指定缓存键所在的命名空间。</param>
        /// <param name="keyRoot">指定用于构建缓存键的前缀。</param>
        /// <param name="provider">缓存提供器。</param>
        /// <param name="expiration">设定缓存的超时时间。</param>
        public CacheOperation(
            string cacheNamespace, string keyRoot, ICacheProvider provider, CacheExpiration expiration)
            : base(cacheNamespace, keyRoot, provider, expiration)
        {
        }

        /// <summary>
        /// 获取一个用于操作给定的缓存键的对象。
        /// </summary>
        /// <param name="v1">用于构建缓存键的第1个数据。</param>
        /// <param name="v2">用于构建缓存键的第2个数据。</param>
        /// <returns>操作给定的缓存键的实例。</returns>
        public KeyOperation<TRes> Key(T1 v1, T2 v2)
        {
            var key = CacheUtils.BuildCacheKey(KeyBase,
                ReferenceEquals(v1, null) ? string.Empty : v1.ToString(),
                ReferenceEquals(v2, null) ? string.Empty : v2.ToString());
            return new KeyOperation<TRes>(this, key);
        }
    }

    /// <summary>
    /// 包含缓存的操作，并提供强类型的缓存键值的管理。
    /// </summary>
    /// <typeparam name="TRes">缓存的值的类型。</typeparam>
    /// <typeparam name="T1">用于构建缓存键的第1个数据的类型。</typeparam>
    /// <typeparam name="T2">用于构建缓存键的第2个数据的类型。</typeparam>
    /// <typeparam name="T3">用于构建缓存键的第3个数据的类型。</typeparam>
    public class CacheOperation<T1, T2, T3, TRes> : CacheOperationBase
    {
        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="cacheNamespace">指定缓存键所在的命名空间。</param>
        /// <param name="keyRoot">指定用于构建缓存键的前缀。</param>
        /// <param name="provider">缓存提供器。</param>
        /// <param name="expiration">设定缓存的超时时间。</param>
        public CacheOperation(
            string cacheNamespace, string keyRoot, ICacheProvider provider, CacheExpiration expiration)
            : base(cacheNamespace, keyRoot, provider, expiration)
        {
        }

        /// <summary>
        /// 获取一个用于操作给定的缓存键的对象。
        /// </summary>
        /// <param name="v1">用于构建缓存键的第1个数据。</param>
        /// <param name="v2">用于构建缓存键的第2个数据。</param>
        /// <param name="v3">用于构建缓存键的第3个数据。</param>
        /// <returns>操作给定的缓存键的实例。</returns>
        public KeyOperation<TRes> Key(T1 v1, T2 v2, T3 v3)
        {
            var key = CacheUtils.BuildCacheKey(KeyBase,
                ReferenceEquals(v1, null) ? string.Empty : v1.ToString(),
                ReferenceEquals(v2, null) ? string.Empty : v2.ToString(),
                ReferenceEquals(v3, null) ? string.Empty : v3.ToString());
            return new KeyOperation<TRes>(this, key);
        }
    }

    /// <summary>
    /// 包含缓存的操作，并提供强类型的缓存键值的管理。
    /// </summary>
    /// <typeparam name="TRes">缓存的值的类型。</typeparam>
    /// <typeparam name="T1">用于构建缓存键的第1个数据的类型。</typeparam>
    /// <typeparam name="T2">用于构建缓存键的第2个数据的类型。</typeparam>
    /// <typeparam name="T3">用于构建缓存键的第3个数据的类型。</typeparam>
    /// <typeparam name="T4">用于构建缓存键的第4个数据的类型。</typeparam>
    public class CacheOperation<T1, T2, T3, T4, TRes> : CacheOperationBase
    {
        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="cacheNamespace">指定缓存键所在的命名空间。</param>
        /// <param name="keyRoot">指定用于构建缓存键的前缀。</param>
        /// <param name="provider">缓存提供器。</param>
        /// <param name="expiration">设定缓存的超时时间。</param>
        public CacheOperation(
            string cacheNamespace, string keyRoot, ICacheProvider provider, CacheExpiration expiration)
            : base(cacheNamespace, keyRoot, provider, expiration)
        {
        }

        /// <summary>
        /// 获取一个用于操作给定的缓存键的对象。
        /// </summary>
        /// <param name="v1">用于构建缓存键的第1个数据。</param>
        /// <param name="v2">用于构建缓存键的第2个数据。</param>
        /// <param name="v3">用于构建缓存键的第3个数据。</param>
        /// <param name="v4">用于构建缓存键的第4个数据。</param>
        /// <returns>操作给定的缓存键的实例。</returns>
        public KeyOperation<TRes> Key(T1 v1, T2 v2, T3 v3, T4 v4)
        {
            var key = CacheUtils.BuildCacheKey(KeyBase,
                ReferenceEquals(v1, null) ? string.Empty : v1.ToString(),
                ReferenceEquals(v2, null) ? string.Empty : v2.ToString(),
                ReferenceEquals(v3, null) ? string.Empty : v3.ToString(),
                ReferenceEquals(v4, null) ? string.Empty : v4.ToString());
            return new KeyOperation<TRes>(this, key);
        }
    }

    /// <summary>
    /// 包含缓存的操作，并提供强类型的缓存键值的管理。
    /// </summary>
    /// <typeparam name="TRes">缓存的值的类型。</typeparam>
    /// <typeparam name="T1">用于构建缓存键的第1个数据的类型。</typeparam>
    /// <typeparam name="T2">用于构建缓存键的第2个数据的类型。</typeparam>
    /// <typeparam name="T3">用于构建缓存键的第3个数据的类型。</typeparam>
    /// <typeparam name="T4">用于构建缓存键的第4个数据的类型。</typeparam>
    /// <typeparam name="T5">用于构建缓存键的第5个数据的类型。</typeparam>
    public class CacheOperation<T1, T2, T3, T4, T5, TRes> : CacheOperationBase
    {
        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="cacheNamespace">指定缓存键所在的命名空间。</param>
        /// <param name="keyRoot">指定用于构建缓存键的前缀。</param>
        /// <param name="provider">缓存提供器。</param>
        /// <param name="expiration">设定缓存的超时时间。</param>
        public CacheOperation(
            string cacheNamespace, string keyRoot, ICacheProvider provider, CacheExpiration expiration)
            : base(cacheNamespace, keyRoot, provider, expiration)
        {
        }

        /// <summary>
        /// 获取一个用于操作给定的缓存键的对象。
        /// </summary>
        /// <param name="v1">用于构建缓存键的第1个数据。</param>
        /// <param name="v2">用于构建缓存键的第2个数据。</param>
        /// <param name="v3">用于构建缓存键的第3个数据。</param>
        /// <param name="v4">用于构建缓存键的第4个数据。</param>
        /// <param name="v5">用于构建缓存键的第5个数据。</param>
        /// <returns>操作给定的缓存键的实例。</returns>
        public KeyOperation<TRes> Key(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5)
        {
            var key = CacheUtils.BuildCacheKey(KeyBase, v1, v2, v3, v4, v5);
            return new KeyOperation<TRes>(this, key);
        }
    }

    /// <summary>
    /// 包含缓存的操作，并提供强类型的缓存键值的管理。
    /// </summary>
    /// <typeparam name="TRes">缓存的值的类型。</typeparam>
    /// <typeparam name="T1">用于构建缓存键的第1个数据的类型。</typeparam>
    /// <typeparam name="T2">用于构建缓存键的第2个数据的类型。</typeparam>
    /// <typeparam name="T3">用于构建缓存键的第3个数据的类型。</typeparam>
    /// <typeparam name="T4">用于构建缓存键的第4个数据的类型。</typeparam>
    /// <typeparam name="T5">用于构建缓存键的第5个数据的类型。</typeparam>
    /// <typeparam name="T6">用于构建缓存键的第6个数据的类型。</typeparam>
    public class CacheOperation<T1, T2, T3, T4, T5, T6, TRes> : CacheOperationBase
    {
        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="cacheNamespace">指定缓存键所在的命名空间。</param>
        /// <param name="keyRoot">指定用于构建缓存键的前缀。</param>
        /// <param name="provider">缓存提供器。</param>
        /// <param name="expiration">设定缓存的超时时间。</param>
        public CacheOperation(
            string cacheNamespace, string keyRoot, ICacheProvider provider, CacheExpiration expiration)
            : base(cacheNamespace, keyRoot, provider, expiration)
        {
        }

        /// <summary>
        /// 获取一个用于操作给定的缓存键的对象。
        /// </summary>
        /// <param name="v1">用于构建缓存键的第1个数据。</param>
        /// <param name="v2">用于构建缓存键的第2个数据。</param>
        /// <param name="v3">用于构建缓存键的第3个数据。</param>
        /// <param name="v4">用于构建缓存键的第4个数据。</param>
        /// <param name="v5">用于构建缓存键的第5个数据。</param>
        /// <param name="v6">用于构建缓存键的第6个数据。</param>
        /// <returns>操作给定的缓存键的实例。</returns>
        public KeyOperation<TRes> Key(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6)
        {
            var key = CacheUtils.BuildCacheKey(KeyBase, v1, v2, v3, v4, v5, v6);
            return new KeyOperation<TRes>(this, key);
        }
    }

    /// <summary>
    /// 包含缓存的操作，并提供强类型的缓存键值的管理。
    /// </summary>
    /// <typeparam name="TRes">缓存的值的类型。</typeparam>
    /// <typeparam name="T1">用于构建缓存键的第1个数据的类型。</typeparam>
    /// <typeparam name="T2">用于构建缓存键的第2个数据的类型。</typeparam>
    /// <typeparam name="T3">用于构建缓存键的第3个数据的类型。</typeparam>
    /// <typeparam name="T4">用于构建缓存键的第4个数据的类型。</typeparam>
    /// <typeparam name="T5">用于构建缓存键的第5个数据的类型。</typeparam>
    /// <typeparam name="T6">用于构建缓存键的第6个数据的类型。</typeparam>
    /// <typeparam name="T7">用于构建缓存键的第7个数据的类型。</typeparam>
    public class CacheOperation<T1, T2, T3, T4, T5, T6, T7, TRes> : CacheOperationBase
    {
        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="cacheNamespace">指定缓存键所在的命名空间。</param>
        /// <param name="keyRoot">指定用于构建缓存键的前缀。</param>
        /// <param name="provider">缓存提供器。</param>
        /// <param name="expiration">设定缓存的超时时间。</param>
        public CacheOperation(
            string cacheNamespace, string keyRoot, ICacheProvider provider, CacheExpiration expiration)
            : base(cacheNamespace, keyRoot, provider, expiration)
        {
        }

        /// <summary>
        /// 获取一个用于操作给定的缓存键的对象。
        /// </summary>
        /// <param name="v1">用于构建缓存键的第1个数据。</param>
        /// <param name="v2">用于构建缓存键的第2个数据。</param>
        /// <param name="v3">用于构建缓存键的第3个数据。</param>
        /// <param name="v4">用于构建缓存键的第4个数据。</param>
        /// <param name="v5">用于构建缓存键的第5个数据。</param>
        /// <param name="v6">用于构建缓存键的第6个数据。</param>
        /// <param name="v7">用于构建缓存键的第7个数据。</param>
        /// <returns>操作给定的缓存键的实例。</returns>
        public KeyOperation<TRes> Key(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7)
        {
            var key = CacheUtils.BuildCacheKey(KeyBase, v1, v2, v3, v4, v5, v6, v7);
            return new KeyOperation<TRes>(this, key);
        }
    }

    /// <summary>
    /// 包含缓存的操作，并提供强类型的缓存键值的管理。
    /// </summary>
    /// <typeparam name="TRes">缓存的值的类型。</typeparam>
    /// <typeparam name="T1">用于构建缓存键的第1个数据的类型。</typeparam>
    /// <typeparam name="T2">用于构建缓存键的第2个数据的类型。</typeparam>
    /// <typeparam name="T3">用于构建缓存键的第3个数据的类型。</typeparam>
    /// <typeparam name="T4">用于构建缓存键的第4个数据的类型。</typeparam>
    /// <typeparam name="T5">用于构建缓存键的第5个数据的类型。</typeparam>
    /// <typeparam name="T6">用于构建缓存键的第6个数据的类型。</typeparam>
    /// <typeparam name="T7">用于构建缓存键的第7个数据的类型。</typeparam>
    /// <typeparam name="T8">用于构建缓存键的第8个数据的类型。</typeparam>
    public class CacheOperation<T1, T2, T3, T4, T5, T6, T7, T8, TRes> : CacheOperationBase
    {
        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="cacheNamespace">指定缓存键所在的命名空间。</param>
        /// <param name="keyRoot">指定用于构建缓存键的前缀。</param>
        /// <param name="provider">缓存提供器。</param>
        /// <param name="expiration">设定缓存的超时时间。</param>
        public CacheOperation(
            string cacheNamespace, string keyRoot, ICacheProvider provider, CacheExpiration expiration)
            : base(cacheNamespace, keyRoot, provider, expiration)
        {
        }

        /// <summary>
        /// 获取一个用于操作给定的缓存键的对象。
        /// </summary>
        /// <param name="v1">用于构建缓存键的第1个数据。</param>
        /// <param name="v2">用于构建缓存键的第2个数据。</param>
        /// <param name="v3">用于构建缓存键的第3个数据。</param>
        /// <param name="v4">用于构建缓存键的第4个数据。</param>
        /// <param name="v5">用于构建缓存键的第5个数据。</param>
        /// <param name="v6">用于构建缓存键的第6个数据。</param>
        /// <param name="v7">用于构建缓存键的第7个数据。</param>
        /// <param name="v8">用于构建缓存键的第8个数据。</param>
        /// <returns>操作给定的缓存键的实例。</returns>
        public KeyOperation<TRes> Key(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8)
        {
            var key = CacheUtils.BuildCacheKey(KeyBase, v1, v2, v3, v4, v5, v6, v7, v8);
            return new KeyOperation<TRes>(this, key);
        }
    }
}
