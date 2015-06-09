using System;
using System.Text;

namespace cmstar.Caching
{
    internal static class CacheUtils
    {
        /// <summary>
        /// 用于在本地内存缓存中表示<c>null</c>。
        /// </summary>
        /// <remarks>
        /// 例如<see cref="System.Web.HttpRuntime.Cache"/>不支持缓存null，为了能使null被缓存下来，
        /// 需用一个特殊的对象类表示null。
        /// </remarks>
        internal static readonly object NullValue = new object();

        /// <summary>
        /// 获取缓存的键。
        /// </summary>
        /// <param name="keyBase">缓存键的前缀部分。</param>
        /// <param name="element">用于构建缓存键的数据。</param>
        /// <returns>缓存的键。</returns>
        public static string BuildCacheKey(string keyBase, object element)
        {
            var sb = new StringBuilder(keyBase);
            sb.Append(':').Append(ToString(element));

            return sb.ToString();
        }

        /// <summary>
        /// 获取缓存的键。
        /// </summary>
        /// <param name="keyBase">缓存键的前缀部分。</param>
        /// <param name="element1">用于构建缓存键的第1个数据。</param>
        /// <param name="element2">用于构建缓存键的第2个数据。</param>
        /// <returns>缓存的键。</returns>
        public static string BuildCacheKey(string keyBase, object element1, object element2)
        {
            var sb = new StringBuilder(keyBase);
            sb.Append(':').Append(ToString(element1))
                .Append('_').Append(ToString(element2));

            return sb.ToString();
        }

        /// <summary>
        /// 获取缓存的键。
        /// </summary>
        /// <param name="keyBase">缓存键的前缀部分。</param>
        /// <param name="element1">用于构建缓存键的第1个数据。</param>
        /// <param name="element2">用于构建缓存键的第2个数据。</param>
        /// <param name="element3">用于构建缓存键的第3个数据。</param>
        /// <returns>缓存的键。</returns>
        public static string BuildCacheKey(
            string keyBase, object element1, object element2, object element3)
        {
            var sb = new StringBuilder(keyBase);
            sb.Append(':').Append(ToString(element1))
                .Append('_').Append(ToString(element2))
                .Append('_').Append(ToString(element3));

            return sb.ToString();
        }

        /// <summary>
        /// 获取缓存的键。
        /// </summary>
        /// <param name="keyBase">缓存键的前缀部分。</param>
        /// <param name="element1">用于构建缓存键的第1个数据。</param>
        /// <param name="element2">用于构建缓存键的第2个数据。</param>
        /// <param name="element3">用于构建缓存键的第3个数据。</param>
        /// <param name="element4">用于构建缓存键的第4个数据。</param>
        /// <returns>缓存的键。</returns>
        public static string BuildCacheKey(
            string keyBase, object element1, object element2, object element3, object element4)
        {
            var sb = new StringBuilder(keyBase);
            sb.Append(':').Append(ToString(element1))
                .Append('_').Append(ToString(element2))
                .Append('_').Append(ToString(element3))
                .Append('_').Append(ToString(element4));

            return sb.ToString();
        }

        /// <summary>
        /// 获取缓存的键。
        /// </summary>
        /// <param name="keyBase">缓存键的前缀部分。</param>
        /// <param name="elements">用于构建缓存键的数据的集合。</param>
        /// <returns>缓存的键。</returns>
        public static string BuildCacheKey(string keyBase, object[] elements)
        {
            var sb = new StringBuilder(keyBase);

            if (elements != null)
            {
                sb.Append(':');

                for (int i = 0; i < elements.Length; i++)
                {
                    if (i > 0) sb.Append('_');

                    var e = elements[i];
                    sb.Append(ToString(e));
                }
            }

            return sb.ToString();
        }

        private static string ToString(object element)
        {
            if (element == null)
                return string.Empty;

            if (element is bool)
                return ((bool)element) ? "1" : "0";

            if (element is DateTime)
                return ((DateTime)element).Ticks.ToString();

            return element.ToString();
        }
    }
}
