namespace cmstar.Caching
{
    /// <summary>
    /// 存储缓存的值。
    /// </summary>
    /// <remarks>
    /// <see cref="Value"/>可能被直接修改，当前类型必须是 class ，不能是 struct 。
    /// </remarks>
    internal class CacheValue
    {
        public CacheValue(object value)
        {
            Value = value;
        }

        public object Value;
    }
}
