using System;
using System.Web;
using System.Web.Caching;

namespace cmstar.Caching
{
    /// <summary>
    /// 基于<see cref="HttpRuntime"/>的缓存提供器实现。
    /// </summary>
    public class HttpRuntimeCacheProvider : ICacheProvider
    {
        /// <summary>
        /// 获取<see cref="HttpRuntimeCacheProvider"/>的唯一实例。
        /// </summary>
        public static readonly HttpRuntimeCacheProvider Instance = new HttpRuntimeCacheProvider();

        private HttpRuntimeCacheProvider() { }

        public T Get<T>(string key)
        {
            var v = HttpRuntime.Cache.Get(key);
            return v == null || ReferenceEquals(CacheUtils.NullValue, v) ? default(T) : (T)v;
        }

        public bool TryGet<T>(string key, out T value)
        {
            var v = HttpRuntime.Cache.Get(key);

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
                ? DateTime.MaxValue
                : DateTime.Now.Add(expiration);

            object v = value;
            HttpRuntime.Cache.Insert(
                key, v ?? CacheUtils.NullValue, null,
                e, Cache.NoSlidingExpiration);
        }

        public bool Remove(string key)
        {
            return HttpRuntime.Cache.Remove(key) != null;
        }
    }
}
