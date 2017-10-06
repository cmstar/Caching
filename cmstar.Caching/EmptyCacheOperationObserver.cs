namespace cmstar.Caching
{
    /// <summary>
    /// 一个什么也不做的<see cref="ICacheOperationObserver"/>实现。
    /// </summary>
    public class EmptyCacheOperationObserver : ICacheOperationObserver
    {
        /// <summary>
        /// 获取类<see cref="EmptyCacheOperationObserver"/>的唯一实例。
        /// </summary>
        public static readonly EmptyCacheOperationObserver Instance = new EmptyCacheOperationObserver();

        /// <inheritdoc />
        public void Accessed(string keyBase)
        {
        }

        /// <inheritdoc />
        public void Hit(string keyBase)
        {
        }

        /// <inheritdoc />
        public void Missed(string keyBase)
        {
        }

        /// <inheritdoc />
        public void Removed(string keyBase)
        {
        }
    }
}