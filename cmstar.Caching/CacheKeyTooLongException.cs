using System;

namespace cmstar.Caching
{
    /// <summary>
    /// 表示缓存的键过长时的错误。
    /// </summary>
    public class CacheKeyTooLongException : Exception
    {
        /// <summary>
        /// 初始化类型的新实例。
        /// </summary>
        /// <param name="length">引发此异常的键的长度。</param>
        public CacheKeyTooLongException(int length)
            : base(BuildMessage(length))
        {
        }

        private static string BuildMessage(int length)
        {
            var msg = $"The cache key (length {length}) is too long, the max length is {CacheEnv.CacheKeyMaxLength}.";
            return msg;
        }
    }
}
