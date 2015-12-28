using System;

namespace cmstar.Caching
{
    /// <summary>
    /// 表示缓存的键过长时的错误。
    /// </summary>
    public class CacheKeyTooLongException : Exception
    {
        public CacheKeyTooLongException(int length)
            : base(BuildMessage(length))
        {
        }

        private static string BuildMessage(int length)
        {
            var msg = string.Format(
                "The cache key (length {0}) is too long, the max length is {1}.",
                length, CacheEnv.CacheKeyMaxLength);
            return msg;
        }
    }
}
