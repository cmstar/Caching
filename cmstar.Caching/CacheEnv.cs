namespace cmstar.Caching
{
    /// <summary>
    /// 包含在缓存体系中使用的基础信息定义。
    /// </summary>
    public static class CacheEnv
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
        /// 用于表示<c>null</c>在缓存中作为字符串存储时的替代。
        /// </summary>
        public const string NullValueString = "NIL*Zrx40vMTx23vnGxnA";

        /// <summary>
        /// 定义缓存的键的最大长度。
        /// </summary>
        public const int CacheKeyMaxLength = 2048;
    }
}
