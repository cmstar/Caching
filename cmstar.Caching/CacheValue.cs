namespace cmstar.Caching
{
    /// <summary>
    /// 存储缓存的值。
    /// </summary>
    internal class CacheValue
    {
        public CacheValue(object value)
        {
            Value = value;
        }

        public object Value;
    }
}
