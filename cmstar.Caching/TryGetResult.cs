namespace cmstar.Caching
{
    /// <summary>
    /// 缓存的获取结果。
    /// </summary>
    /// <typeparam name="T">缓存值的类型。</typeparam>
    public struct TryGetResult<T>
    {
        /// <summary>
        /// 创建<see cref="TryGetResult{T}"/>。
        /// </summary>
        public TryGetResult(bool hasValue, T value)
        {
            HasValue = hasValue;
            Value = value;
        }

        /// <summary>
        /// 若获取到了缓存的值，返回true；否则返回false。
        /// </summary>
        public bool HasValue { get; }

        /// <summary>
        /// 当<see cref="HasValue"/>为true时，存放获取到的缓存的值。否则为<typeparamref name="T"/>的默认值。
        /// </summary>
        public T Value { get; }
    }
}
