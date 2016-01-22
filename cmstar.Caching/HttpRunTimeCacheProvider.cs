using System;
using System.Web;
using System.Web.Caching;

namespace cmstar.Caching
{
    /// <summary>
    /// 基于<see cref="HttpRuntime"/>的缓存提供器实现。
    /// </summary>
    public class HttpRuntimeCacheProvider : MemoryBasedCacheProvider
    {
        /// <summary>
        /// 获取<see cref="HttpRuntimeCacheProvider"/>的唯一实例。
        /// </summary>
        public static readonly HttpRuntimeCacheProvider Instance = new HttpRuntimeCacheProvider();

        private HttpRuntimeCacheProvider() { }

        protected override T DoGet<T>(string key)
        {
            var v = HttpRuntime.Cache.Get(key);
            return v == null || ReferenceEquals(CacheEnv.NullValue, v) ? default(T) : (T)v;
        }

        protected override bool DoTryGet<T>(string key, out T value)
        {
            var v = HttpRuntime.Cache.Get(key);

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
                ? DateTime.MaxValue
                : DateTime.Now.Add(expiration);
            var v = (object)value ?? CacheEnv.NullValue;
            HttpRuntime.Cache.Insert(key, v, null, e, Cache.NoSlidingExpiration);
        }

        protected override bool DoRemove(string key)
        {
            return HttpRuntime.Cache.Remove(key) != null;
        }
    }
}
