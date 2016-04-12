using System;
using System.Collections;
using System.Text;

namespace cmstar.Caching
{
    /// <summary>
    /// 提供了用于组装缓存键的辅助方法。
    /// </summary>
    internal static class CacheUtils
    {
        /// <summary>
        /// 获取缓存的键。
        /// </summary>
        /// <param name="keyBase">缓存键的前缀部分。</param>
        /// <param name="element">用于构建缓存键的数据。</param>
        /// <returns>缓存的键。</returns>
        public static string BuildCacheKey(string keyBase, object element)
        {
            var sb = new StringBuilder(keyBase);
            sb.Append(':').ExAppend(element);

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
            sb.Append(':').ExAppend(element1)
                .Append('_').ExAppend(element2);

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
            sb.Append(':').ExAppend(element1)
                .Append('_').ExAppend(element2)
                .Append('_').ExAppend(element3);

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
            sb.Append(':').ExAppend(element1)
                .Append('_').ExAppend(element2)
                .Append('_').ExAppend(element3)
                .Append('_').ExAppend(element4);

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
                    if (i > 0)
                    {
                        sb.Append('_');
                    }

                    var e = elements[i];
                    sb.ExAppend(e);
                }
            }

            return sb.ToString();
        }

        private static StringBuilder ExAppend(this StringBuilder sb, object v)
        {
            if (v == null)
            {
                sb.Append(CacheEnv.NullValueString);
                return sb;
            }

            var escape = false;
            var s = v as string;

            if (s != null)
            {
                escape = true;
            }
            else if (v is bool)
            {
                s = ((bool)v) ? "1" : "0";
            }
            else if (v is DateTime)
            {
                s = ((DateTime)v).Ticks.ToString();
            }
            else if (v is Guid)
            {
                s = ((Guid)v).ToString("N");
            }
            else if (v is Enum)
            {
                s = Convert.ToInt64(v).ToString();
            }
            else
            {
                // 对于复杂类型，仅兼容数组（序列）
                var array = v as IEnumerable;
                if (array != null)
                {
                    AppendArray(sb, array);
                    return sb;
                }

                s = v.ToString();
                escape = true;
            }

            if (!escape)
            {
                sb.Append(s);
                return sb;
            }

            var len = s.Length;
            for (int i = 0; i < len; i++)
            {
                var c = s[i];

                switch (c)
                {
                    case '\\': // 转义符自身
                    case '_':  // 各参数的分隔符
                    case '~':  // 数组元素分隔符
                        sb.Append('\\');
                        break;
                }

                sb.Append(c);
            }

            if (sb.Length > CacheEnv.CacheKeyMaxLength)
                throw new CacheKeyTooLongException(sb.Length);

            return sb;
        }

        private static void AppendArray(StringBuilder sb, IEnumerable elements)
        {
            /*
             * 序列以形如 [1~2~3] 的方式组装，即以[]包含，元素间使用~分隔，
             * ~字符是需要转义的，而[]不需要转义，为了避免与字符串"[]"间的歧义，
             * 对于空序列，也拼入~，使其成为[~]，以便与字符串区分开来，
             * 因为字符串的"[~]"会被转义为"[\~]"
             */

            sb.Append('[');

            var hasElement = false;
            foreach (var e in elements)
            {
                if (hasElement)
                {
                    sb.Append('~');
                }
                else
                {
                    hasElement = true;
                }

                sb.ExAppend(e);
            }

            if (!hasElement)
            {
                sb.Append('~');
            }

            sb.Append(']');
        }
    }
}
