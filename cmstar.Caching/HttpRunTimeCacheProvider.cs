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

        protected override object DoGet(string key)
        {
            var v = HttpRuntime.Cache.Get(key);
            return v;
        }

        protected override void DoSet(string key, object value, TimeSpan expiration)
        {
            var e = TimeSpan.Zero.Equals(expiration)
                ? DateTime.MaxValue
                : DateTime.Now.Add(expiration);
            HttpRuntime.Cache.Insert(key, value, null, e, Cache.NoSlidingExpiration);
        }

        protected override bool DoCreate(string key, object value, TimeSpan expiration)
        {
            var e = TimeSpan.Zero.Equals(expiration)
                ? DateTime.MaxValue
                : DateTime.Now.Add(expiration);

            var oldValue = HttpRuntime.Cache.Add(
                key, value, null, e, Cache.NoSlidingExpiration, CacheItemPriority.Default, null);

            return oldValue == null;
        }

        protected override bool DoRemove(string key)
        {
            return HttpRuntime.Cache.Remove(key) != null;
        }
    }
}
