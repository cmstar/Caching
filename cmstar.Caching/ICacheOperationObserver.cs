namespace cmstar.Caching
{
    /// <summary>
    /// 定义一组方法，用于接收<see cref="CacheOperation{TRes}"/>的有关操作的结果。
    /// 可以通过这些方法对缓存缓存操作进行统计。
    /// </summary>
    public interface ICacheOperationObserver
    {
        /// <summary>
        /// 当发生了一次缓存操作时，且没有调用<see cref="Hit"/>、<see cref="Missed"/>、
        /// <see cref="Removed"/>时，调用此方法。
        /// </summary>
        /// <param name="keyBase">缓存键的前缀部分，对应<see cref="CacheOperationBase.KeyBase"/>。</param>
        void Accessed(string keyBase);

        /// <summary>
        /// 当读取一次缓存，且命中了该缓存（缓存键存在），此方法被调用。
        /// </summary>
        /// <param name="keyBase">缓存键的前缀部分，对应<see cref="CacheOperationBase.KeyBase"/>。</param>
        /// <remarks>
        /// 读取操作包含以下方法及对应的异步版本：
        /// <see cref="ICacheProvider.Get{T}"/>、
        /// <see cref="ICacheProvider.TryGet{T}"/>、
        /// <see cref="ICacheFieldAccessable.FieldGet{T,TField}"/>、
        /// <see cref="ICacheFieldAccessable.FieldTryGet{T,TField}"/>。
        /// </remarks>
        void Hit(string keyBase);

        /// <summary>
        /// 当读取一次缓存，且未命中该缓存（缓存键不存在），此方法被调用。
        /// </summary>
        /// <param name="keyBase">缓存键的前缀部分，对应<see cref="CacheOperationBase.KeyBase"/>。</param>
        /// <remarks>
        /// 读取操作包含以下方法及对应的异步版本：
        /// <see cref="ICacheProvider.Get{T}"/>、
        /// <see cref="ICacheProvider.TryGet{T}"/>、
        /// <see cref="ICacheFieldAccessable.FieldGet{T,TField}"/>、
        /// <see cref="ICacheFieldAccessable.FieldTryGet{T,TField}"/>。
        /// </remarks>
        void Missed(string keyBase);

        /// <summary>
        /// 当一个已存在的缓存被移除时，此方法被调用。
        /// </summary>
        /// <param name="keyBase">缓存键的前缀部分，对应<see cref="CacheOperationBase.KeyBase"/>。</param>
        /// <remarks>
        /// 当<see cref="ICacheProvider.Remove"/>返回<c>true</c>时，一个缓存即被移除。
        /// </remarks>
        void Removed(string keyBase);
    }
}
